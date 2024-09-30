using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using RedmineTableEditor.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Telerik.Windows.Controls;

namespace RedmineTableEditor.Models.FileSettings
{
    public class SubIssueSettingsModel : IssueSettingsModelBase
    {
        public ObservableCollection<SubIssueSettingModel> Items { get; set; }

        public SubIssueSettingsModel() : base()
        {
            Items = new ObservableCollection<SubIssueSettingModel>();
        }
        public List<GridViewColumnGroup> CreateSubIssueColumnGroups()
        {
            return Items.OrderBy(a => a.Order).Where(a => a.IsEnabled).Select(b => new GridViewColumnGroup()
            {
                Name = b.Order.ToString(),
                Header = b.Title
            }).ToList();
        }

        public List<GridViewBoundColumnBase> CreateColumns(RedmineManager redmine)
        {
            var subIssueColumns = new List<GridViewBoundColumnBase>();
            foreach (var sub in Items.Where(a => a.IsEnabled && a.IsValid).OrderBy(a => a.Order))
            {
                subIssueColumns.AddRange(Properties
                    .Where(a => a.Field.HasValue || a.MyField.HasValue || redmine.Cache.CustomFields.Any(b => b.IsEnabled() && b.Id == a.CustomFieldId))
                    .Select(a => a.CreateColumn(redmine, sub.Order))
                    .Where(a => a != null));
            }
            return subIssueColumns;
        }
    }
}
