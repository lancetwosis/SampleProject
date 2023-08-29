using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using AutoMapper;
using RedmineTimePuncher.ViewModels.Settings.Bases;
using RedmineTimePuncher.Enums;
using LibRedminePower.Helpers;
using System.Reactive.Linq;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class RedmineSettingsViewModel : SettingsViewModelBase<RedmineSettingsModel>
    {
        public NeedsRestartSettingViewModel<LocaleType> Locale { get; set; }
        public List<LocaleType> Locales { get; set; }
        public ReactivePropertySlim<string> UrlBase { get; set; }
        public ReactivePropertySlim<string> UserName { get; set; }
        public ReactivePropertySlim<string> Password { get; set; }
        public ReactivePropertySlim<string> AdminApiKey { get; set; }
        public ReactivePropertySlim<int> ConcurrencyMax { get; set; }

        public ReactivePropertySlim<bool> UseBasicAuth { get; set; }
        public ReactivePropertySlim<string> ApiKey { get; set; }
        public ReactivePropertySlim<string> UserNameOfBasicAuth { get; set; }
        public ReactivePropertySlim<string> PasswordOfBasicAuth { get; set; }

        public RedmineSettingsViewModel(RedmineSettingsModel model) :base(model)
        {
            Locale = new NeedsRestartSettingViewModel<LocaleType>(model.ToReactivePropertySlimAsSynchronized(a => a.Locale).AddTo(disposables)).AddTo(disposables);
            Locales = FastEnumUtility.FastEnum.GetValues<LocaleType>().Where(t => t != LocaleType.Unselected).ToList();

            UrlBase = model.ToReactivePropertySlimAsSynchronized(a => a.UrlBase).AddTo(disposables);
            UserName = model.ToReactivePropertySlimAsSynchronized(a => a.UserName).AddTo(disposables);
            Password = model.ToReactivePropertySlimAsSynchronized(a => a.Password).AddTo(disposables);
            AdminApiKey = model.ToReactivePropertySlimAsSynchronized(a => a.AdminApiKey).AddTo(disposables);
            ConcurrencyMax = model.ToReactivePropertySlimAsSynchronized(a => a.ConcurrencyMax).AddTo(disposables);

            UseBasicAuth = model.ToReactivePropertySlimAsSynchronized(a => a.UseBasicAuth).AddTo(disposables);
            ApiKey = model.ToReactivePropertySlimAsSynchronized(a => a.ApiKey).AddTo(disposables);
            UserNameOfBasicAuth = model.ToReactivePropertySlimAsSynchronized(a => a.UserNameOfBasicAuth).AddTo(disposables);
            PasswordOfBasicAuth = model.ToReactivePropertySlimAsSynchronized(a => a.PasswordOfBasicAuth).AddTo(disposables);
        }
    }
}
