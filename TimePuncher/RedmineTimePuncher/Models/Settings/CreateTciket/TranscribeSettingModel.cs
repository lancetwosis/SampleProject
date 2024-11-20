using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket
{
    public class TranscribeSettingModel : Bases.SettingsModelBase<TranscribeSettingModel>
    {
        public static MyCustomFieldPossibleValue NOT_SPECIFIED_PROCESS = new MyCustomFieldPossibleValue()
        {
            IsDefault = true,
            Label = LibRedminePower.Properties.Resources.SettingsNotSpecified,
            Value = "-1",
        };

        public bool IsEnabled { get; set; }

        public ObservableCollection<TranscribeSettingItemModel> Items { get; set; } = new ObservableCollection<TranscribeSettingItemModel>();

        public TranscribeSettingModel()
        { }

        public async Task<string> TranscribeAsync(MyIssue target, MyCustomFieldPossibleValue process)
        {
            if (!IsEnabled || !CacheManager.Default.MarkupLang.CanTranscribe())
                return null;

            var setting = Items.FirstOrDefault(i => i.NeedsTranscribe(target, process));
            if (setting == null)
                return null;
            if (!setting.IsValid())
                throw new ApplicationException(Resources.SettingsReviErrMsgInvalidTranscribeSetting);

            MyWikiPage wiki = null;
            try
            {
                wiki = await Task.Run(() => RedmineManager.Default.Value.GetWikiPage(setting.WikiProject.Id.ToString(), setting.WikiPage.Title));
            }
            catch
            {
                throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindWikiPage, setting.WikiPage.Title));
            }

            var lines = wiki.GetSectionLines(CacheManager.Default.MarkupLang, setting.Header, setting.IncludesHeader).Select(l => l.Text).ToList();
            return string.Join(Environment.NewLine, lines);
        }
    }
}
