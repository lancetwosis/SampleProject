using LibRedminePower.Applications;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels;
using RedmineTimePuncher.Views;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;

namespace RedmineTimePuncher
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Any(a => a == Logger.DECRYPT_KEY))
            {
                Logger.DecryptLogs();
                Environment.Exit(0);
            }

            // 初期化
            LibRedminePower.Initializer.Init(this);
            TraceHelper.TrackAtomicFeature(ApplicationInfo.Title.Replace(" ", "") + ".Start");

            // スプラッシュ画面も言語切り替えを行いたいのでこのタイミングで実施する
            SettingsModel.InitLocale();

            // スプラッシュ画面
            var dataContext = (SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext;
            dataContext.ImagePath = "pack://application:,,,/RedmineStudio;component/Resources/splashScreen_redminetimepunhcer.png";
            dataContext.Footer = string.Format(LibRedminePower.Properties.Resources.SplashScreenFooter, ApplicationInfo.Title);
            RadSplashScreenManager.Show();

            // 前バージョンからのUpgradeを実行していないときは、Upgradeを実施する
            if (RedmineTimePuncher.Properties.Settings.Default.IsUpgrade == false)
            {
                updateUserConfigDirectory();

                RedmineTimePuncher.Properties.Settings.Default.Upgrade();
                RedmineTableEditor.Properties.Settings.Default.Upgrade();

                RedmineTimePuncher.Properties.Settings.Default.IsUpgrade = true;
                RedmineTimePuncher.Properties.Settings.Default.Save();
            }
            RedmineTimePuncher.Properties.Settings.Default.AppVersion = ApplicationInfo.Version.ToString();

            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel(e.Args);
            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }

        // 難読化対応により user.config の格納されるフォルダ名が変更されてしまう現象が発生した。
        // フォルダ名は RedmineTimePuncher.exe_Url_[ハッシュ値] のように自動でつけられるが、難読化対応により [ハッシュ値] が変わってしまった。
        // よって、設定を引き継ぐため、過去の設定フォルダをコピペしてくる処理を行う。
        private void updateUserConfigDirectory()
        {
            var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

            var myConfDir = new DirectoryInfo(Path.GetDirectoryName(Path.GetDirectoryName(conf.FilePath)));
            // 使用中のフォルダにすでに自分より古いバージョンの設定フォルダがあった場合、何もしない
            if (myConfDir.Exists && myConfDir.GetDirectories().Any(v => new Version(v.Name) < ApplicationInfo.Version))
                return;

            var parentDir = new DirectoryInfo(Path.GetDirectoryName(myConfDir.FullName));
            // AppData\Local\RedminePower が存在しない場合、完全な初回インストールなので、何もしない
            if (!parentDir.Exists)
                return;

            // RedmineStudioまたはTimePuncher(名称変更前) の設定フォルダのみを対象とし、
            var tpConfDirs = parentDir.GetDirectories().Where(d => d.Name.StartsWith("RedmineStudio.exe") || d.Name.StartsWith("RedmineTimePuncher.exe")).ToList();

            // その中から自分のバージョンに最も近い過去バージョンの設定フォルダを取得する
            var preVerDir = tpConfDirs.Where(d => d.Name != myConfDir.Name)
                .SelectMany(d => d.GetDirectories().Select(v => (VerDir: v, Version: new Version(v.Name))))
                .Where(v => v.Version < ApplicationInfo.Version)
                .OrderBy(v => v.Version).LastOrDefault();
            if (preVerDir.VerDir == null)
                return;

            // フォルダを作成し、最も近い過去バージョンの user.config をその中にコピーする
            if (!myConfDir.Exists)
                Directory.CreateDirectory(myConfDir.FullName);
            var myPreVerDir = Directory.CreateDirectory(Path.Combine(myConfDir.FullName, preVerDir.VerDir.Name));
            File.Copy(Path.Combine(preVerDir.VerDir.FullName, "user.config"), Path.Combine(myPreVerDir.FullName, "user.config"));
        }
    }
}
