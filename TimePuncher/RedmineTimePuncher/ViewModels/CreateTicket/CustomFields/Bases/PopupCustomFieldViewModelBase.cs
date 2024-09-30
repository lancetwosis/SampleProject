using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Bases
{
    public class PopupCustomFieldViewModelBase : CustomFieldViewModelBase
    {
        public bool NowEditing { get; set; }
        public ReactiveCommand EditCommand { get; set; }
        public ReactiveCommand SaveCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public PopupCustomFieldViewModelBase(CustomField cf) : base(cf)
        {
        }

        protected override void setup(CustomField cf, MyIssue ticket)
        {
            EditCommand = new ReactiveCommand().WithSubscribe(() => NowEditing = true).AddTo(disposables);
            SaveCommand = new ReactiveCommand().WithSubscribe(() => NowEditing = false).AddTo(disposables);
            DeleteCommand = new ReactiveCommand().WithSubscribe(() => deleteValue()).AddTo(disposables);
        }

        protected virtual void deleteValue()
        {
            throw new NotImplementedException();
        }
    }
}
