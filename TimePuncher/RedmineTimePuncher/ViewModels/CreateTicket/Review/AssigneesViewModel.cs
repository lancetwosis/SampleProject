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
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class AssigneesViewModel : AssigneesViewModelBase<TargetTicketModel>
    {
        public AssigneesViewModel(ObservableCollection<AssigneeModel> assignees, TargetTicketModel target)
            : base(assignees, target, Resources.ReviewReviewer)
        {
        }

        public void ApplyTemplate(ObservableCollection<AssigneeModel> template)
        {
            if (template.IsNotEmpty())
            {
                SelectedAssignees.Clear();
                foreach (var ta in template)
                {
                    var assignee = AllAssignees.Value.FirstOrDefault(a => a.Model.Id == ta.Id);
                    if (assignee == null)
                        continue;

                    assignee.IsRequired.Value = ta.IsRequired;
                    SelectedAssignees.Add(assignee);
                }
            }
        }
    }
}
