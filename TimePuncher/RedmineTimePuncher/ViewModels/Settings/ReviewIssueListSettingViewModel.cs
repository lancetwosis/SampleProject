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
    public class ReviewIssueListSettingViewModel : Bases.SettingsViewModelBase<ReviewIssueListSettingModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReactivePropertySlim<bool> ShowDescription { get; set; }
        public ReactivePropertySlim<bool> ShowLastNote { get; set; }

        public TwinListBoxViewModel<IssueProperty> Properties { get; set; }

        public ReviewIssueListSettingViewModel(ReviewIssueListSettingModel issueList,
            ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(issueList)
        {
            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                await setupAsync(r, issueList);
            }).AddTo(disposables);

            ErrorMessage = new IObservable<string>[] { errorMessage, issueList.IsBusy }
                .CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(async () =>
            {
                await setupAsync(redmine.Value, issueList);
            }).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        private async Task setupAsync(RedmineManager r, ReviewIssueListSettingModel issueList)
        {
            try
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                await issueList.SetupAsync(r);

                ShowDescription = issueList.ToReactivePropertySlimAsSynchronized(m => m.ShowDescription).AddTo(myDisposables);
                ShowLastNote = issueList.ToReactivePropertySlimAsSynchronized(m => m.ShowLastNote).AddTo(myDisposables);
                Properties = new TwinListBoxViewModel<IssueProperty>(issueList.AllProperties, issueList.SelectedProperties).AddTo(myDisposables);
            }
            catch (Exception ex)
            {
                issueList.IsBusy.Value = ex.Message;
            }
        }
    }
}
