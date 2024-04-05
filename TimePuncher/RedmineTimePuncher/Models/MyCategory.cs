using LibRedminePower.Extentions;
using Reactive.Bindings;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Models
{
    public class MyCategory : Category
    {
        public int Id { get; set; }
        public bool IsEnabled => Model.IsEnabled;
        public CategorySettingModel Model { get; set; }
        public SolidColorBrush ForeBrush { get; }
        public TimeEntryActivity TimeEntry { get; }

        public bool IsPined { get; set; }
        public ReactiveCommand<RadToggleButton> PinCommand { get; set; }

        public MyCategory(CategorySettingModel model)
        {
            this.Model = model;
            this.Id = model.Id;
            this.TimeEntry = model.TimeEntry;
            this.CategoryName = model.TimeEntry.Name;
            this.DisplayName = model.TimeEntry.Name;
            this.CategoryBrush = model.Color.ToBrush();
            this.ForeBrush = model.ForeColor.ToBrush();
        }

        /// <summary>
        /// 各プロジェクトの時間管理で設定されている作業時間に対するコンストラクタ。
        /// システム作業分類の設定が外れていると Id が「選択肢の値」の Id （= model の Id）と異なるため、こちらを使用する。
        /// </summary>
        public MyCategory(CategorySettingModel model, ProjectTimeEntryActivity entry) : this(model)
        {
            this.Id = entry.Id;
        }

        public MyCategory(CategorySettingModel model, bool isPined, ReactiveCommand<RadToggleButton> pinCommand) : this(model)
        {
            IsPined = isPined;
            PinCommand = pinCommand;
        }

        /// <summary>
        /// 引数には、対象のチケットを含み、その親チケットを再帰的に取得したリストを指定すること。
        /// その際、リストは親チケット→子チケットの順でソートされていること。
        /// </summary>
        public bool IsMatch(List<MyIssue> issues, int myUserId, bool isAutoSameName)
        {
            if (isAutoSameName)
            {
                var result = issues.Select(a => a.Subject).Contains(this.DisplayName);
                if (result) return result;
            }
            return Model.IsMatch(issues, myUserId);
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
