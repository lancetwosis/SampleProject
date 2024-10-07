using AutoMapper;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class CategorySettingsModel : Bases.SettingsModelBase<CategorySettingsModel>
    {
        public ObservableCollection<CategorySettingModel> Items { get; set; } = new ObservableCollection<CategorySettingModel>();
        public bool IsAutoSameName { get; set; } = true;

        public CategorySettingsModel()
        { }

        public void UpdateItems(List<TimeEntryActivity> timeEntries)
        {
            var indexed = timeEntries.Select((v, i) => (v, i)).ToList();

            // 更新
            var upList = indexed.Where(a => Items.Any(b => b.Id == a.v.Id)).ToList();
            upList.ForEach(a =>
            {
                Items.First(b => b.Id == a.v.Id).Setup(a.v);
            });

            // 追加
            var addList = indexed.Where(a => !Items.Any(b => b.Id == a.v.Id)).ToList();
            addList.ForEach(a => Items.Add(new CategorySettingModel(a.v)));

            // 削除
            var removeList = Items.Where(a => !timeEntries.Any(b => b.Id == a.Id)).ToList();
            removeList.ForEach(a => Items.Remove(a));
        }

        public bool IsWorkingTime(MyTimeEntry entry)
        {
            var item = Items.FirstOrDefault(i => i.Id == entry.Activity.Id);
            return item != null ? item.IsWorkingTime : true;
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CategorySettingsModel, CategorySettingsModel>()
                .AfterMap((src, dest) =>
                {
                    // AutoMapperは、コレクション型プロパティでは、既存のインスタンスをそのまま利用して上書きコピーを行うため、
                    // プロパティの参照が変わらない限り PropertyChanged イベントは発行されない。
                    // 新しいインスタンスに置き換えることで、PropertyChanged イベントを発行させる
                    dest.Items = new ObservableCollection<CategorySettingModel>(src.Items);
                });
        }
    }
}
