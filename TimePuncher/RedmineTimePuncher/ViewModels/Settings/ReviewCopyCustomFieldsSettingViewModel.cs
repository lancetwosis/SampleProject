using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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

        public TwinListBoxViewModel<MyCustomField> CustomFields { get; set; }

        public ReviewCopyCustomFieldsSettingViewModel(ReviewCopyCustomFieldsSettingModel copyCustomFields,
            ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(copyCustomFields)
        {
            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                await setupAsync(r, copyCustomFields);
            }).AddTo(disposables);

            ErrorMessage = new IObservable<string>[] { errorMessage, copyCustomFields.IsBusy }
                .CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(async () =>
            {
                await setupAsync(redmine.Value, copyCustomFields);
            }).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        private async Task setupAsync(RedmineManager r, ReviewCopyCustomFieldsSettingModel copyCustomFields)
        {
            try
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                await copyCustomFields.SetupAsync(r);

                CustomFields = new TwinListBoxViewModel<MyCustomField>(copyCustomFields.AllCustomFields, copyCustomFields.SelectedCustomFields).AddTo(myDisposables);
            }
            catch (Exception ex)
            {
                copyCustomFields.IsBusy.Value = ex.Message;
            }
        }
    }
}
