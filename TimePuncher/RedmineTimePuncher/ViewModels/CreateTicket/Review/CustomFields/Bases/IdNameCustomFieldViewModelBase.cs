using LibRedminePower.Extentions;
using LibRedminePower.Models;
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

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases
{
    public class IdNameCustomFieldViewModelBase<T> : CustomFieldViewModelBase where T : IdName
    {
        public List<T> Values { get; set; }
        public ObservableCollection<T> SelectedValues { get; set; }
        public ReactivePropertySlim<T> SelectedValue { get; set; }

        public IdNameCustomFieldViewModelBase(CustomFieldValueModel cf, MyIssue ticket) : base(cf, ticket)
        {
        }

        protected override void setup(CustomFieldValueModel cf, MyIssue ticket = null)
        {
            var values = getValues(ticket);
            if (Model.CustomField.Multiple)
            {
                Values = values;
                SelectedValues = new ObservableCollection<T>();
                SelectedValues.CollectionChangedAsObservable().StartWithDefault().SubscribeWithErr(_ =>
                {
                    Model.Value = string.Join(",", SelectedValues.Select(u => u.Id));
                }).AddTo(disposables);
            }
            else
            {
                if (!Model.CustomField.IsRequired)
                {
                    values.Insert(0, getNotSpecified());
                }
                Values = values;
                SelectedValue = new ReactivePropertySlim<T>().AddTo(disposables);
                SelectedValue.SubscribeWithErr(v =>
                {
                    Model.Value = (v == null || v.Id == IdName.INVALID_ID) ? "" : v.Id.ToString();
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
                rawValue = IdName.INVALID_ID.ToString();

            if (Model.CustomField.Multiple)
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
                SelectedValue.Value = Values.FirstOrDefault(a => a.Id.ToString() == rawValue);
            }
        }
    }
}
