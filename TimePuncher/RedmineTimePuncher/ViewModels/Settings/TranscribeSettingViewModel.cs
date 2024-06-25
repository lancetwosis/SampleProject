using LibRedminePower;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels.Bases;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using LibRedminePower.ViewModels;
using RedmineTimePuncher.Extentions;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class TranscribeSettingViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public EditableGridViewModel<TranscribeSettingItemModel> Items { get; set; }
        public ReactiveCommand TestCommand { get; set; }

        public TranscribeSettingViewModel(ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> isBusy)
        {
            ErrorMessage = redmine.Select(r => r != null && !CacheManager.Default.GetTemporaryMarkupLang().CanTranscribe())
                                  .Select(a => a ? Resources.SettingsReviErrMsgCannotUseTranscribe : null)
                                  .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        public void Setup(TranscribeSettingModel transcribe, ReactivePropertySlim<string> isBusy)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            IsEnabled = transcribe.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(myDisposables);

            Items = new EditableGridViewModel<TranscribeSettingItemModel>(transcribe.Items).AddTo(myDisposables);
            Items.CollectionChangedAsObservable().StartWithDefault().SubscribeWithErr(async e =>
            {
                if (e?.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var i in e.NewItems.OfType<TranscribeSettingItemModel>())
                    {
                        await i.SetupAsync(isBusy);
                    }
                }
            }).AddTo(myDisposables);

            TestCommand = Items.SelectedItem.Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                var selectedItem = Items.SelectedItem.Value;
                if (!selectedItem.IsValid())
                    throw new ApplicationException(Resources.SettingsReviErrMsgInvalidTranscribeSetting);

                MyWikiPage wiki = null;
                try
                {
                    isBusy.Value = Resources.SettingsMsgNowGettingData;
                    wiki = TranscribeSettingModel.REDMINE.GetWikiPage(selectedItem.Project.Id.ToString(), selectedItem.WikiPage.Title);
                }
                catch
                {
                    throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindWikiPage, selectedItem.WikiPage.Title));
                }
                finally
                {
                    isBusy.Value = null;
                }

                var lines = wiki.GetSectionLines(CacheManager.Default.GetTemporaryMarkupLang(), selectedItem.Header, selectedItem.IncludesHeader);
                MessageBoxHelper.Input(Resources.ReviewMsgTranscribeFollowings, string.Join(Environment.NewLine, lines.Select(l => l.Text)), true);
            }).AddTo(myDisposables);
        }
    }
}
