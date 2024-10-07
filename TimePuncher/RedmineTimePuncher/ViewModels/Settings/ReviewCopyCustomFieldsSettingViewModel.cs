using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class ReviewCopyCustomFieldsSettingViewModel : Bases.SettingsViewModelBase<ReviewCopyCustomFieldsSettingModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyCustomField>> AllCustomFields { get; set; }

        public ReadOnlyReactivePropertySlim<TwinListBoxViewModel<MyCustomField>> CustomFields { get; set; }

        public ReviewCopyCustomFieldsSettingViewModel(ReviewCopyCustomFieldsSettingModel model) : base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            AllCustomFields = CacheTempManager.Default.MyCustomFields.Where(a => a != null)
                .Select(a => a.Select(b => new MyCustomField(b)).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables) ;

            AllCustomFields.Where(a => a != null).CombineLatest(
                model.ObserveProperty(a => a.SelectedCustomFields), (all, sel) => (all, sel)).SubscribeWithErr(a => 
                {
                    var notExists = a.sel.Where(sc => !a.all.Any(c => c.Id == sc.Id)).ToList();
                    foreach (var i in notExists)
                        a.sel.Remove(i);
                }).AddTo(disposables);

            CustomFields =
                AllCustomFields.Where(a => a != null).CombineLatest(
                    model.ObserveProperty(a => a.SelectedCustomFields), (all, sel) =>
                    new TwinListBoxViewModel<MyCustomField>(all, sel)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
