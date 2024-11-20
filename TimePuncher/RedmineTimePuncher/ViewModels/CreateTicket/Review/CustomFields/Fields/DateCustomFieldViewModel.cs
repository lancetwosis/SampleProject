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
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Fields
{
    public class DateCustomFieldViewModel : PopupCustomFieldViewModelBase
    {
        public ReactivePropertySlim<DateTime> Date { get; set; }
        public ReactivePropertySlim<string> DisplayDate { get; set; }

        public DateCustomFieldViewModel(CustomFieldValueModel cf) : base(cf)
        {
        }

        protected override void setup(CustomFieldValueModel cf, MyIssue ticket)
        {
            base.setup(cf, ticket);

            DisplayDate = new ReactivePropertySlim<string>().AddTo(disposables);
            Date = new ReactivePropertySlim<DateTime>(DateTime.Today).AddTo(disposables);

            DisplayDate.SubscribeWithErr(d =>
            {
                if (d == Resources.ReviewCfDateWatermark)
                    return;

                if (DateTime.TryParse(d, out var date))
                    setValue(date);
                else
                    clearValue();
            }).AddTo(disposables);

            Model.ObserveProperty(a => a.Value).SubscribeWithErr(v =>
            {
                if (DateTime.TryParse(v, out var date))
                    DisplayDate.Value = date.ToString("yyyy/MM/dd");
            }).AddTo(disposables);

            Date.SubscribeWithErr(d =>
            {
                // 日付が新たに選択されたらポップアップを閉じる
                if (needsSet) NowEditing = false;
            }).AddTo(disposables);

            this.ObserveProperty(a => a.NowEditing).SubscribeWithErr(nowEditing =>
            {
                if (nowEditing)
                {
                    needsSet = true;
                }
                else
                {
                    if (needsSet)
                        setValue(Date.Value);
                    else
                        clearValue();
                }
            }).AddTo(disposables);
        }

        private void setValue(DateTime date)
        {
            Model.Value = date.ToString("yyyy-MM-dd");
            Date.Value = date;
        }

        private void clearValue()
        {
            DisplayDate.Value = Resources.ReviewCfDateWatermark;
            Model.Value = null;
            Date.Value = DateTime.Today;
        }

        private bool needsSet = true;
        protected override void deleteValue()
        {
            needsSet = false;
            NowEditing = false;
        }

        public override void SetValue(string value)
        {
            if (DateTime.TryParse(value, out var date))
                setValue(date);
            else
                clearValue();
        }
    }
}
