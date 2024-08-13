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
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Fields
{
    public class ListCustomFieldViewModel : CustomFieldViewModelBase
    {
        public List<string> Values { get; set; }
        public ObservableCollection<string> SelectedValues { get; set; }

        public ListCustomFieldViewModel(CustomField cf) : base(cf)
        {
        }

        protected override void setup(CustomField cf, MyIssue ticket = null)
        {
            if (CustomField.Multiple)
            {
                Values = cf.PossibleValues.Select(v => v.Value).ToList();
                SelectedValues = new ObservableCollection<string>();
                SelectedValues.CollectionChangedAsObservable().StartWithDefault().SubscribeWithErr(_ =>
                {
                    Value = string.Join(",", SelectedValues);
                }).AddTo(disposables);
            }
            else
            {
                var values = cf.PossibleValues.Select(v => v.Value).ToList();
                if (!CustomField.IsRequired)
                {
                    values.Insert(0, "");
                }
                Values = values;
            }
        }

        public override void SetValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (CustomField.Multiple)
            {
                SelectedValues.Clear();
                foreach (var v in value.Split(','))
                {
                    var pv = Values.FirstOrDefault(p => p == v);
                    if (pv != null)
                        SelectedValues.Add(pv);
                }
            }
            else
            {
                var pv = Values.FirstOrDefault(p => p == value);
                if (pv != null)
                    Value = pv;
            }
        }
    }

}
