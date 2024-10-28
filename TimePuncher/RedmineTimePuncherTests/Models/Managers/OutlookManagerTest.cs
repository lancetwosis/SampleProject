using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using System;

namespace RedmineTimePuncherTests.Models.Managers
{
    [TestClass]
    public class OutlookManagerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            SettingsModel.Default = new SettingsModel() {
                Appointment = new AppointmentSettingsModel() {
                    Outlook = new AppointmentOutlookSettingsModel(){
                        IsEnabled = true
                    }
                }
            };

            var outlook = OutlookManager.Default.Value;

            SettingsModel.Default.Appointment.Outlook.IsEnabled = false;

            Assert.IsNull(OutlookManager.Default.Value);

            SettingsModel.Default.Appointment.Outlook.IsEnabled = true;

            Assert.IsNotNull(OutlookManager.Default.Value);
        }
    }
}
