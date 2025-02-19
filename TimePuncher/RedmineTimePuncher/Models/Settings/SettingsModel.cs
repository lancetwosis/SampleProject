using AutoMapper;
using LibRedminePower.Extentions;
using LibRedminePower.Localization;
using Reactive.Bindings;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.CreateTicket;
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
        private static SettingsModel _default = Read();
        public static SettingsModel Default
        {
            get { return _default; }
            set 
            {
                // 変更があったプロパティのみを更新する。
                if (!_default.Redmine.Equals(value.Redmine)) // ToJsonは個人情報を除去して比較してしまうので、使えない。
                    _default.Redmine = value.Redmine;
                if (_default.Schedule.ToJson() != value.Schedule.ToJson())
                    _default.Schedule = value.Schedule;
                if (_default.Calendar.ToJson() != value.Calendar.ToJson())
                    _default.Calendar = value.Calendar;
                if (_default.Category.ToJson() != value.Category.ToJson())
                    _default.Category = value.Category;
                if (_default.Appointment.ToJson() != value.Appointment.ToJson())
                    _default.Appointment = value.Appointment;
                if (_default.Query.ToJson() != value.Query.ToJson())
                    _default.Query = value.Query;
                if (_default.User.ToJson() != value.User.ToJson())
                    _default.User = value.User;
                if (_default.OutputData.ToJson() != value.OutputData.ToJson())
                    _default.OutputData = value.OutputData;
                if (_default.CreateTicket.ToJson() != value.CreateTicket.ToJson())
                    _default.CreateTicket = value.CreateTicket;
                if (_default.ReviewIssueList.ToJson() != value.ReviewIssueList.ToJson())
                    _default.ReviewIssueList = value.ReviewIssueList;
                if (_default.ReviewCopyCustomFields.ToJson() != value.ReviewCopyCustomFields.ToJson())
                    _default.ReviewCopyCustomFields = value.ReviewCopyCustomFields;
                if (_default.TranscribeSettings.ToJson() != value.TranscribeSettings.ToJson())
                    _default.TranscribeSettings = value.TranscribeSettings;
                if (_default.RequestWork.ToJson() != value.RequestWork.ToJson())
                    _default.RequestWork = value.RequestWork;
                if (_default.PersonHourReport.ToJson() != value.PersonHourReport.ToJson())
                    _default.PersonHourReport = value.PersonHourReport;
            }
        }

        public RedmineSettingsModel Redmine { get; set; } = new RedmineSettingsModel();
        public ScheduleSettingsModel Schedule { get; set; } = new ScheduleSettingsModel();
        public CategorySettingsModel Category { get; set; } = new CategorySettingsModel();
        public CalendarSettingsModel Calendar { get; set; } = new CalendarSettingsModel();
        public AppointmentSettingsModel Appointment { get; set; } = new AppointmentSettingsModel();
        public QuerySettingsModel Query { get; set; } = new QuerySettingsModel();
        public UserSettingsModel User { get; set; } = new UserSettingsModel();
        public OutputDataSettingsModel OutputData { get; set; } = new OutputDataSettingsModel();
        public CreateTicketSettingsModel CreateTicket { get; set; } = new CreateTicketSettingsModel();
        public TranscribeSettingsModel TranscribeSettings { get; set; } = new TranscribeSettingsModel();
        public ReviewIssueListSettingModel ReviewIssueList { get; set; } = new ReviewIssueListSettingModel();
        public ReviewCopyCustomFieldsSettingModel ReviewCopyCustomFields { get; set; } = new ReviewCopyCustomFieldsSettingModel();
        public RequestWorkSettingsModel RequestWork { get; set; } = new RequestWorkSettingsModel();
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

                // 以前は、いくつかの接続情報をSettings.Default配下で管理しており、それを設定クラスに読み込む処理が必要だったが、RedmineSettingクラスで関係するように移行した。
                if (!Properties.Settings.Default.IsMigratedForRedmineSettingsModel)
                {
                    result.Redmine.LoadProperties();
                    Properties.Settings.Default.IsMigratedForRedmineSettingsModel = true;
                }
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
                Properties.Settings.Default.SaveWithErr();
            }

            if (result.Redmine.Locale == LocaleType.Unselected || result.Redmine.Locale != current)
            {
                result.Redmine.Locale = current;
                Properties.Settings.Default.Setting = result.ToJson();
                Properties.Settings.Default.SaveWithErr();
            }

            return result;
        }

        public void Save()
        {
            Properties.Settings.Default.Setting = this.ToJson();
            Properties.Settings.Default.SaveWithErr();
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            // 何も指定しないと、インスタンスを維持しながらコピーするので、
            // 以下のコードを入れることで、プロパティ単位でのコピーを指定している。
            cfg.CreateMap<SettingsModel, SettingsModel>();

            // Redmine設定情報は、個人情報はコピーしない。
            Redmine.SetupConfigure(cfg);
        }
    }
}
