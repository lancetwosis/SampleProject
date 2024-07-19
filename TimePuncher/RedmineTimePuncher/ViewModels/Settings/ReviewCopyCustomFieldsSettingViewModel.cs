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
            ErrorMessage = new IObservable<string>[] { errorMessage, copyCustomFields.IsBusy }
                .CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(_ => setup(copyCustomFields)).AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(() => setup(copyCustomFields)).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        private void setup(ReviewCopyCustomFieldsSettingModel copyCustomFields)
        {
            try
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                copyCustomFields.Setup();

                CustomFields = new TwinListBoxViewModel<MyCustomField>(copyCustomFields.AllCustomFields, copyCustomFields.SelectedCustomFields).AddTo(myDisposables);
            }
            catch (Exception ex)
            {
                copyCustomFields.IsBusy.Value = ex.Message;
            }
        }
    }
}
