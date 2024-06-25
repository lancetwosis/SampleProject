using LibRedminePower.Extentions;
using LibRedminePower.Localization;
using RedmineTimePuncher.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Models.Settings
{
    public class SettingsModel : Bases.SettingsModelBase<SettingsModel>
    {
        public RedmineSettingsModel Redmine { get; set; } = new RedmineSettingsModel();
        public ScheduleSettingsModel Schedule { get; set; } = new ScheduleSettingsModel();
        public CategorySettingsModel Category { get; set; } = new CategorySettingsModel();
        public AppointmentSettingsModel Appointment { get; set; } = new AppointmentSettingsModel();
        public QuerySettingsModel Query { get; set; } = new QuerySettingsModel();
        public UserSettingsModel User { get; set; } = new UserSettingsModel();
        public OutputDataSettingsModel OutputData { get; set; } = new OutputDataSettingsModel();
        public CreateTicketSettingsModel CreateTicket { get; set; } = new CreateTicketSettingsModel();
        public TranscribeSettingsModel TranscribeSettings { get; set; } = new TranscribeSettingsModel();
        public ReviewIssueListSettingModel ReviewIssueList { get; set; } = new ReviewIssueListSettingModel();
        public ReviewCopyCustomFieldsSettingModel ReviewCopyCustomFields { get; set; } = new ReviewCopyCustomFieldsSettingModel();
        public RequestWorkSettingsModel RequestWork { get; set; } = new RequestWorkSettingsModel();
        public CalendarSettingsModel Calendar { get; set; } = new CalendarSettingsModel();
        public PersonHourReportSettingsModel PersonHourReport { get; set; } = new PersonHourReportSettingsModel();

        public SettingsModel()
        { }

        public static void InitLocale()
        {
            var locale = Properties.Settings.Default.CurrentLocale.ToLocaleType();
            if (locale == LocaleType.Unselected)
            {
                locale = LocaleTypeEx.GetCurrent();
            }

            var culture = locale.ToCultureInfo();
            try
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                // 以下のような曜日の英語、日本語の切り替えができなかったため本処理を追加
                // "{Binding CurrentDate.Value,
                //           StringFormat=yyyy / MM / dd(ddd),
                //           ConverterCulture=ja-JP}"
                // https://stackoverflow.com/questions/4041197/how-to-set-and-change-the-culture-in-wpf
                var lang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(lang));

                // 日本語の場合のみ Telerik の日本語設定を行う
                LocalizationManager.Manager = culture.IsJp() ? new MyLocalizationManager() : null;
            }
            catch
            {
                // 例外は無視
            }
        }

        public static SettingsModel Read()
        {
            SettingsModel result = null;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting))
            {
                result = CloneExtentions.ToObject<SettingsModel>(Properties.Settings.Default.Setting);
                result.Redmine.LoadProperties();
            }
            else
            {
                result = new SettingsModel();
            }

            var current = Properties.Settings.Default.CurrentLocale.ToLocaleType();
            if (current == LocaleType.Unselected)
            {
                current = LocaleTypeEx.GetCurrent();
                Properties.Settings.Default.CurrentLocale = current.ToString();
                Properties.Settings.Default.Save();
            }

            if (result.Redmine.Locale == LocaleType.Unselected || result.Redmine.Locale != current)
            {
                result.Redmine.Locale = current;
                Properties.Settings.Default.Setting = result.ToJson();
                Properties.Settings.Default.Save();
            }

            return result;
        }

        public void Save()
        {
            Properties.Settings.Default.Setting = this.ToJson();
            Redmine.SaveProperties();
            Properties.Settings.Default.Save();
        }
    }
}
