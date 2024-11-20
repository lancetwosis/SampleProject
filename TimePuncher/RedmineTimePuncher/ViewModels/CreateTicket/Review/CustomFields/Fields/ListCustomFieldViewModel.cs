using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Fields
{
    public class ListCustomFieldViewModel : CustomFieldViewModelBase
    {
        public List<string> Values { get; set; }
        public ObservableCollection<string> SelectedValues { get; set; }

        private static string EMPTY_VALUE { get; } = "";

        public ListCustomFieldViewModel(CustomFieldValueModel cf) : base(cf)
        {
        }

        protected override void setup(CustomFieldValueModel cf, MyIssue ticket = null)
        {
            if (Model.CustomField.Multiple)
            {
                Values = Model.CustomField.PossibleValues.Select(v => v.Value).ToList();

                SelectedValues = new ObservableCollection<string>();
                SelectedValues.CollectionChangedAsObservable().StartWithDefault().SubscribeWithErr(_ =>
                {
                    Model.Value = string.Join(",", SelectedValues);
                }).AddTo(disposables);
            }
            else
            {
                var values = Model.CustomField.PossibleValues.Select(v => v.Value).ToList();
                if (!Model.CustomField.IsRequired)
                {
                    values.Insert(0, EMPTY_VALUE);
                }
                Values = values;
            }
        }

        public override void SetValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                value = EMPTY_VALUE;

            if (Model.CustomField.Multiple)
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
                Model.Value = Values.FirstOrDefault(p => p == value);
            }
        }
    }

}
