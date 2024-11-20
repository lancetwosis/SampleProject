using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases
{
    public abstract class AssigneesViewModelBase<TTargetModel> : ViewModelBase
        where TTargetModel : TargetTicketModelBase
    {
        public ObservableCollection<AssigneeViewModel> SelectedAssignees { get; set; }
        public ReadOnlyReactivePropertySlim<List<AssigneeViewModel>> AllAssignees { get; set; }
        public ReadOnlyReactivePropertySlim<AssigneesTwinListViewModel> TwinList { get; set; }

        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        protected AssigneesViewModelBase(ObservableCollection<AssigneeModel> assignees, TTargetModel target, string assigneeType)
        {
            SelectedAssignees = new ObservableCollectionSync<AssigneeViewModel, AssigneeModel>(assignees,
                m => m != null ? new AssigneeViewModel(m) : null,
                vm => vm?.Model).AddTo(disposables);
            AllAssignees = target.ObserveProperty(m => m.Ticket).CombineLatest(CacheManager.Default.Updated, (t, _) => t).Select(t =>
            {
                if (t == null)
                    return new List<AssigneeViewModel>();

                var memberships = CacheManager.Default.ProjectMemberships.TryGetValue(t.Project.Id, out var ms) ? ms : new List<ProjectMembership>();
                var allAssignees = memberships.Select(m => new AssigneeViewModel(m.User)).ToList();
                if (!allAssignees.Any())
                    throw new ApplicationException(Resources.ReviewErrMsgNoMemberAssgined);

                return allAssignees;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            TwinList = AllAssignees.Where(a => a != null).Select(all =>
            {
                var prevReviewers = SelectedAssignees.ToList();
                SelectedAssignees.Clear();
                return new AssigneesTwinListViewModel(all, SelectedAssignees, prevReviewers);
            }).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsValid = SelectedAssignees.AnyAsObservable()
                .Select(any => any ? null : string.Format(Resources.ReviewErrMsgSelectXXX, assigneeType))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
