using AutoMapper;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Data;

namespace RedmineTimePuncher.Models.Settings
{
    public class QuerySettingsModel : Bases.SettingsModelBase<QuerySettingsModel>, IAutoUpdateSetting
    {
        public bool IsAutoUpdate { get; set; } = false;
        public int AutoUpdateMinutes { get; set; } = 15;
        public ObservableCollection<MyQuery> Items { get; set; } = new ObservableCollection<MyQuery>();

        public void UpdateItems(List<MyQuery> queries)
        {
            var target = queries.Where(a => Items.Any(b => a.Id == b.Id)).ToList();

            Items.Clear();
            target.ToList().ForEach(a => Items.Add(a));
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QuerySettingsModel, QuerySettingsModel>()
                .AfterMap((src, dest) =>
                {
                    // AutoMapperは、コレクション型プロパティでは、既存のインスタンスをそのまま利用して上書きコピーを行うため、
                    // プロパティの参照が変わらない限り PropertyChanged イベントは発行されない。
                    // 新しいインスタンスに置き換えることで、PropertyChanged イベントを発行させる
                    dest.Items = new ObservableCollection<MyQuery>(src.Items);
                });
        }
    }
}
