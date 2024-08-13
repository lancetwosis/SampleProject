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

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Bases
{
    public class IdNameCustomFieldViewModelBase<T> : CustomFieldViewModelBase where T : IdName
    {
        public List<T> Values { get; set; }
        public ObservableCollection<T> SelectedValues { get; set; }
        public ReactivePropertySlim<T> SelectedValue { get; set; }

        public IdNameCustomFieldViewModelBase(CustomField cf, MyIssue ticket) : base(cf, ticket)
        {
        }

        protected override void setup(CustomField cf, MyIssue ticket = null)
        {
            var values = getValues(ticket);
            if (CustomField.Multiple)
            {
                Values = values;
                SelectedValues = new ObservableCollection<T>();
                SelectedValues.CollectionChangedAsObservable().StartWithDefault().SubscribeWithErr(_ =>
                {
                    Value = string.Join(",", SelectedValues.Select(u => u.Id));
                }).AddTo(disposables);
            }
            else
            {
                if (!CustomField.IsRequired)
                {
                    values.Insert(0, getNotSpecified());
                }
                Values = values;
                SelectedValue = new ReactivePropertySlim<T>().AddTo(disposables);
                SelectedValue.Where(i => i != null).SubscribeWithErr(i =>
                {
                    Value = i.Id == IdName.INVALID_ID ? "" : i.Id.ToString();
                }).AddTo(disposables);
            }
        }

        protected virtual List<T> getValues(MyIssue ticket)
        {
            throw new NotImplementedException();
        }

        protected virtual T getNotSpecified()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return;

            if (CustomField.Multiple)
            {
                SelectedValues.Clear();
                foreach (var v in rawValue.Split(','))
                {
                    var value = Values.FirstOrDefault(a => a.Id.ToString() == v);
                    if (value != null)
                        SelectedValues.Add(value);
                }
            }
            else
            {
                var values = Values.FirstOrDefault(a => a.Id.ToString() == rawValue);
                if (values != null)
                    SelectedValue.Value = values;
            }
        }
    }
}
