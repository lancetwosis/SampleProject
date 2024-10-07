using AutoMapper;
using LibRedminePower.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class UserSettingsModel : Bases.SettingsModelBase<UserSettingsModel>
    {
        public ObservableCollection<MyUser> Items { get; set; } = new ObservableCollection<MyUser>();

        public void UpdateItems(List<MyUser> users)
        {
            var target = users.Where(a => Items.Any(b => a.Id == b.Id)).ToList();

            Items.Clear();
            target.ToList().ForEach(a => Items.Add(a));
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UserSettingsModel, UserSettingsModel>()
                .AfterMap((src, dest) =>
                {
                    // AutoMapperは、コレクション型プロパティでは、既存のインスタンスをそのまま利用して上書きコピーを行うため、
                    // プロパティの参照が変わらない限り PropertyChanged イベントは発行されない。
                    // 新しいインスタンスに置き換えることで、PropertyChanged イベントを発行させる
                    dest.Items = new ObservableCollection<MyUser>(src.Items);
                });
        }
    }
}
