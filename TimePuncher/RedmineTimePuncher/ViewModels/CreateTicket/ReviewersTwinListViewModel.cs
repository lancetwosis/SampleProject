using LibRedminePower.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket
{
    public class ReviewersTwinListViewModel : ExpandableTwinListBoxViewModel<MemberViewModel>
    {
        public ReviewersTwinListViewModel(IEnumerable<MemberViewModel> allItems, ObservableCollection<MemberViewModel> selectedItems, IEnumerable<MemberViewModel> defaultReviewers = null)
            : base(allItems, selectedItems)
        {
            if (defaultReviewers != null)
            {
                foreach (var pre in defaultReviewers)
                {
                    var reviewer = allItems.FirstOrDefault(r => r.User.Id == pre.User?.Id);
                    if (reviewer != null)
                    {
                        reviewer.IsRequiredReviewer.Value = pre.IsRequired == null ?
                                                            true :
                                                            pre.IsRequired.IsTrue();
                        ToItems.Add(reviewer);
                    }
                }
            }
        }

        /// <summary>
        /// チケットが設定されていない状態で起動するとレイアウトが崩れるのでダミーを設定する
        /// </summary>
        public ReviewersTwinListViewModel() : base(new List<MemberViewModel>(), new ObservableCollection<MemberViewModel>())
        {
        }
    }
}
