using LibRedminePower;
using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common
{
    public interface IPreviewViewModel
    {
        ReactivePropertySlim<WebView2> WebView2 { get; set; }
    }

    public class PreviewViewModel<TRequest, TPeriod> : ViewModelBase, IPreviewViewModel
        where TRequest : RequestTicketsModelBase<TPeriod>
        where TPeriod : PeriodModelBase
    {
        public bool IsSupported { get; set; } = true;
        public ReactivePropertySlim<bool> NowNavigating { get; set; }
        public ReactivePropertySlim<WebView2> WebView2 { get; set; }
        public ReactivePropertySlim<string> InputText { get; set; }
        public ReactiveCommandSlim ReloadCommand { get; set; }

        /// <summary>
        /// WebView2 が前面に表示される問題があるため Visibility を自前で制御する
        /// https://github.com/MicrosoftEdge/WebView2Feedback/issues/286
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsVisible { get; set; }

        public PreviewViewModel(TRequest requests, Expression<Func<TRequest, string>> selector)
        {
            NowNavigating = new ReactivePropertySlim<bool>(true).AddTo(disposables);
            WebView2 = new ReactivePropertySlim<WebView2>().AddTo(disposables);
            InputText = requests.ToReactivePropertySlimAsSynchronized(selector).AddTo(disposables);

            ReloadCommand = NowNavigating.Inverse().ToReactiveCommandSlim()
                .WithSubscribe(async () => await reloadAsync()).AddTo(disposables);

            WebView2.Where(a => a != null).SubscribeWithErr(webView2 =>
            {
                webView2.CoreWebView2InitializationCompleted += async (s, e) =>
                {
                    if (e.IsSuccess)
                    {
#if DEBUG
#else
                        webView2.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif

                        webView2.CoreWebView2.WebMessageReceived += (ss, ee) =>
                        {
                            InputText.Value = ee.TryGetWebMessageAsString();
                        };

                        webView2.CoreWebView2.NavigationStarting += (ss, ee) =>
                        {
                            if (ee.Uri.Contains("/login") || ee.Uri.Contains("/issues/new"))
                            {
                                NowNavigating.Value = true;
                            }
                            else
                            {
                                // ログインおよび新規作成以外のページへの遷移は別途ブラウザを起動する
                                ee.Cancel = true;
                                Process.Start(ee.Uri);
                            }
                        };
                        webView2.CoreWebView2.BasicAuthenticationRequested += (ss, ee) =>
                        {
                            ee.Response.UserName = SettingsModel.Default.Redmine.UserNameOfBasicAuth;
                            ee.Response.Password = SettingsModel.Default.Redmine.PasswordOfBasicAuth;
                        };
                        webView2.CoreWebView2.NavigationCompleted += (ss, ee) =>
                        {
                            NowNavigating.Value = false;
                        };
                        webView2.CoreWebView2.NewWindowRequested += (ss, ee) =>
                        {
                            // 新規ウィンドウを開く場合は別途ブラウザを起動する
                            ee.Handled = true;
                            Process.Start(ee.Uri);
                        };

                        await webView2.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
new class {
  constructor() {
    window.document.addEventListener('readystatechange', this.changeState);
  }

  changeState(event) {
    // main 以外の部分を非表示にする
    var topmenu = window.document.getElementById('top-menu');
    if (topmenu) {
      if (topmenu.style.display != 'none')
        topmenu.style.display = 'none';
      else
        // 複数回、実行されることがあるため抑制する
        return;
    }
    var header = window.document.getElementById('header');
    if (header)
      header.style.display = 'none';
    var footer = window.document.getElementById('footer');
    if (footer)
      footer.style.display = 'none';

    // ページの移動は Process.Start(url) に置換しているため確認ダイアログを抑制する
    Object.defineProperty(window, 'onbeforeunload', {
      set(newValue) {
        if (typeof newValue === 'function') window.onbeforeunload = null;
      }
    });

    // 設定のユーザ名とパスワードを使ってログインを試みる
    var username = window.document.getElementById('username');
    if (username) {" + $@"
      username.value = '{SettingsModel.Default.Redmine.UserName}';" + @"
      var password = window.document.getElementById('password');" + $@"
      password.value = '{SettingsModel.Default.Redmine.Password}'" + @"
      var loginForm = window.document.getElementById('login-form');
      loginForm.firstElementChild.submit();
    }

    // 説明の入力のフォームとスクリプトを取り出し、 main.content 配下に再配置する
    var all_attributes = window.document.getElementById('all_attributes');
    if (all_attributes) {
      var form = window.document.getElementById('issue_description_and_toolbar');
      var script = Array.from(all_attributes.childNodes)
        .find(c => c.nodeType == Node.ELEMENT_NODE && c.innerHTML.includes('wikiToolbar = new jsToolBar'));
      form.remove();
      script.remove();

      var content = window.document.getElementById('content');
      Array.from(content.childNodes).filter(c => c.nodeType == Node.ELEMENT_NODE)
        .forEach(c => c.style.display = 'none');
      content.appendChild(form);
      content.appendChild(script);

      // 入力のイベントを刈り取り、内容をアプリ側に送信する
      var textArea = window.document.getElementById('issue_description');
      textArea.addEventListener('input', e => {
        window.chrome.webview.postMessage(textArea.value);
      });
    }
  }
}
");
                    }
                    else
                    {
                        IsSupported = false;
                    }
                };
            });

            WebView2.CombineLatest(RedmineManager.Default, (w, r) => (w, r))
                .SubscribeWithErr(async _ => await reloadAsync()).AddTo(disposables);

            // セッションや Cookie の競合を回避するため個別のプロファイルを作成する
            envTask = CoreWebView2Environment.CreateAsync(
                        userDataFolder: Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        }

        private Task<CoreWebView2Environment> envTask { get; }
        private async Task reloadAsync()
        {
            if (WebView2.Value == null || RedmineManager.Default.Value == null)
                return;

            var url = RedmineManager.Default.Value.CreateNewIssueURL(InputText.Value);
            try
            {
                if (WebView2.Value.CoreWebView2 == null)
                    await WebView2.Value.EnsureCoreWebView2Async(await envTask);

                WebView2.Value.CoreWebView2.Navigate(url);
            }
            catch (Exception e)
            {
                // Windows10 だと WebView2 のランタイムがインストールされておらずエラーになる場合がある
                // https://developer.microsoft.com/ja-jp/microsoft-edge/webview2?form=MA13LH#download
                Logger.Warn(e, "Failed to EnsureCoreWebView2Async of WebView2.");
                IsSupported = false;
            }
        }

        public void Clear()
        {
            InputText.Value = "";

            if (WebView2.Value != null && WebView2.Value.CoreWebView2 != null)
            {
                var _ = WebView2.Value.CoreWebView2.ExecuteScriptAsync(@"
var textArea = window.document.getElementById('issue_description');
if (textArea)
  textArea.value = '';
");
            }
        }

        public void SetIsVisible(BusyTextNotifier isBusy, ReadOnlyReactivePropertySlim<bool> nowSelfReviewing = null)
        {
            if (nowSelfReviewing != null)
            {
                IsVisible = isBusy.CombineLatest(nowSelfReviewing, (i, n) => !i && !n).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            }
            else
            {
                IsVisible = isBusy.Inverse().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            }
        }
    }
}
