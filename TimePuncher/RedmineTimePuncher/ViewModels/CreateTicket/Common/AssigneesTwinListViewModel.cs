using LibRedminePower.ViewModels;
using RedmineTimePuncher.Models.CreateTicket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common
{
    public class AssigneesTwinListViewModel : ExpandableTwinListBoxViewModel<AssigneeViewModel>
    {
        public AssigneesTwinListViewModel(IEnumerable<AssigneeViewModel> allAssignees, ObservableCollection<AssigneeViewModel> selectedAssignees, IEnumerable<AssigneeViewModel> defaultAssignees)
            : base(allAssignees, selectedAssignees)
        {
            foreach (var pre in defaultAssignees)
            {
                // 担当者が有効だった場合のみ追加する
                var assignee = allAssignees.FirstOrDefault(r => r.Model.Id == pre.Model?.Id);
                if (assignee != null)
                {
                    assignee.IsRequired.Value = pre.IsRequired.Value;
                    ToItems.Add(assignee);
                }
            }
        }
    }
}
