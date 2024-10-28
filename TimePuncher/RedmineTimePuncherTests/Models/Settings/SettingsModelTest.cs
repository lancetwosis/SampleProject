using LibRedminePower.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedmineTimePuncherTests.Models.Settings
{
    [TestClass]
    public class SettingsModelTest
    {
        [TestMethod]
        public void ImportTest()
        {
            // インポートされた場合、プロパティ単位でコピーされるか？
            var test = new SettingsModel();
            test.Redmine.UrlBase = "123";

            var tempFileName = Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFileName, test.ToJson());

            var queuePropertyChanged = new Queue<string>();

            var temp = new SettingsModel();
            temp.PropertyChanged += (s, e) => queuePropertyChanged.Enqueue(e.PropertyName);
            temp.Import(tempFileName);

            Assert.IsTrue(queuePropertyChanged.Count > 0);
        }

        [TestMethod]
        public void DefaultCopyRedmineOnly()
        {
            SettingsModel.Default = SettingsModel.Read();
            var test = SettingsModel.Default.Clone();
            test.Redmine.UrlBase = "123";

            var queuePropertyChanged = new List<string>();

            SettingsModel.Default.PropertyChanged += (s, e) => queuePropertyChanged.Add(e.PropertyName);

            SettingsModel.Default = test;

            Assert.IsTrue(
                queuePropertyChanged.Count == 1 &&
                queuePropertyChanged[0] == "Redmine");
        }

        [TestMethod]
        public void DefaultCopyNone()
        {
            SettingsModel.Default = SettingsModel.Read();
            var test = SettingsModel.Default.Clone();

            var queuePropertyChanged = new List<string>();

            SettingsModel.Default.PropertyChanged += (s, e) => queuePropertyChanged.Add(e.PropertyName);

            SettingsModel.Default = test;

            Assert.IsTrue(queuePropertyChanged.Count == 0);
        }

        [TestMethod]
        public void DefaultCopyAppointmentOnly()
        {
            SettingsModel.Default = SettingsModel.Read();
            var test = SettingsModel.Default.Clone();
            test.Appointment.MyWorks.IsAutoUpdate = !test.Appointment.MyWorks.IsAutoUpdate;

            var queuePropertyChanged = new List<string>();

            SettingsModel.Default.PropertyChanged += (s, e) => queuePropertyChanged.Add(e.PropertyName);

            SettingsModel.Default = test;

            Assert.IsTrue(
                queuePropertyChanged.Count == 1 &&
                queuePropertyChanged[0] == "Appointment");
        }
    }
}
