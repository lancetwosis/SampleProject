using AutoMapper;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.Models.Settings.CreateTicket
{
    public class ReviewCopyCustomFieldsSettingModel : Bases.SettingsModelBase<ReviewCopyCustomFieldsSettingModel>
    {
        public ObservableCollection<MyCustomField> SelectedCustomFields { get; set; } = new ObservableCollection<MyCustomField>();

        public ReviewCopyCustomFieldsSettingModel()
        { }

        public List<IssueCustomField> GetCopiedCustomFields(MyIssue parent)
        {
            if (parent.RawIssue.CustomFields == null || parent.RawIssue.CustomFields.IsEmpty())
                return new List<IssueCustomField>();

            return parent.RawIssue.CustomFields.Where(c => SelectedCustomFields.Any(sc => sc.Id == c.Id)).ToList();
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ReviewCopyCustomFieldsSettingModel, ReviewCopyCustomFieldsSettingModel>()
                .AfterMap((src, dest) =>
                {
                    // AutoMapperは、コレクション型プロパティでは、既存のインスタンスをそのまま利用して上書きコピーを行うため、
                    // プロパティの参照が変わらない限り PropertyChanged イベントは発行されない。
                    // 新しいインスタンスに置き換えることで、PropertyChanged イベントを発行させる
                    dest.SelectedCustomFields = new ObservableCollection<MyCustomField>(src.SelectedCustomFields);
                });
        }
    }
}
