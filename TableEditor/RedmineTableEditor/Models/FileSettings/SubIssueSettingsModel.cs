using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace RedmineTableEditor.Models.FileSettings
{
    public class SubIssueSettingsModel : IssueSettingsModelBase
    {
        public ObservableCollection<SubIssueSettingModel> Items { get; set; }

        public SubIssueSettingsModel() : base()
        {
            Items = new ObservableCollection<SubIssueSettingModel>();
        }
    }
}
