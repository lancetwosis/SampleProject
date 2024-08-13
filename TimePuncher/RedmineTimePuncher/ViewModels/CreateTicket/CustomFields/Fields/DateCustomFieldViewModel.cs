using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
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

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Fields
{
    public class DateCustomFieldViewModel : PopupCustomFieldViewModelBase
    {
        public ReactivePropertySlim<DateTime> Date { get; set; }
        public ReactivePropertySlim<string> DisplayDate { get; set; }

        public DateCustomFieldViewModel(CustomField cf) : base(cf)
        {
            DisplayDate = new ReactivePropertySlim<string>().AddTo(disposables);
            DisplayDate.SubscribeWithErr(d =>
            {
                if (d == Resources.ReviewCfDateWatermark)
                    return;

                if (DateTime.TryParse(d, out var date))
                    setValue(date);
                else
                    clearValue();
            }).AddTo(disposables);

            this.ObserveProperty(a => a.Value).SubscribeWithErr(v =>
            {
                if (DateTime.TryParse(v, out var date))
                    DisplayDate.Value = date.ToString("yyyy/MM/dd");
            }).AddTo(disposables);

            Date = new ReactivePropertySlim<DateTime>(DateTime.Today).AddTo(disposables);
            Date.SubscribeWithErr(d =>
            {
                // 日付が新たに選択されたらポップアップを閉じる
                if (isEnabled) NowEditing = false;
            }).AddTo(disposables);

            this.ObserveProperty(a => a.NowEditing).SubscribeWithErr(nowEditing =>
            {
                if (nowEditing)
                {
                    isEnabled = true;
                }
                else
                {
                    if (isEnabled)
                        setValue(Date.Value);
                    else
                        clearValue();
                }
            }).AddTo(disposables);
        }

        private void setValue(DateTime date)
        {
            Value = date.ToString("yyyy-MM-dd");
        }

        private void clearValue()
        {
            DisplayDate.Value = Resources.ReviewCfDateWatermark;
            Value = null;
        }

        private bool isEnabled = true;
        protected override void deleteValue()
        {
            isEnabled = false;
            NowEditing = false;
        }
    }
}
