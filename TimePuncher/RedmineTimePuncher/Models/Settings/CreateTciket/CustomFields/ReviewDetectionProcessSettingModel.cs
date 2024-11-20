using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields
{
    public class ReviewDetectionProcessSettingModel : CustomFieldSettingModelBase
    {
        public ReviewDetectionProcessSettingModel() : base(CustomFieldFormat.List)
        {
            // 「レビュー対象の工程の指定」は必ずカスタムフィールドに保存する必要があるため、有効・無効と連動させる
            this.ObserveProperty(a => a.IsEnabled).SubscribeWithErr(i => NeedsSaveToCustomField = i).AddTo(disposables);
        }
    }
}
