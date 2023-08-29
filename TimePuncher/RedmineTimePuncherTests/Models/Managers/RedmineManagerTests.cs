using LibRedminePower.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Managers.Tests
{
    [TestClass()]
    public class RedmineManagerTests
    {
        [TestMethod()]
        public void RedmineManagerTest()
        {
            var manager = createManager();

            var issue = manager.GetIssueIncludeJournal("107 ");
            var date = DateTime.Parse("2022/02/23 17:43:29");
            var result = manager.RestoreJournals(issue, date);

            Assert.AreEqual("レビュー中", result.Status);
            Assert.AreEqual("6", result.AssignedTo);    // 担当が直哉
        }

        [TestMethod()]
        public void RedmineManagerTest2()
        {
            var manager = createManager();

            var issue = manager.GetIssueIncludeJournal("107 ");
            var date = DateTime.Parse("2022/02/23 17:43:30");
            var result = manager.RestoreJournals(issue, date);

            Assert.AreEqual("進行中", result.Status);
            Assert.AreEqual("5", result.AssignedTo);    // 担当が直哉
        }

        [TestMethod()]
        public void RedmineManagerTest3()
        {
            var manager = createManager();

            var issue = manager.GetIssueIncludeJournal("107 ");
            var date = DateTime.Parse("2022/02/23 17:43:28");
            var result = manager.RestoreJournals(issue, date);

            Assert.AreEqual("レビュー中", result.Status);
            Assert.AreEqual("6", result.AssignedTo);    // 担当が直哉
        }

        private static RedmineManager createManager()
        {
            var model = new RedmineSettingsModel();
            model.UrlBase = "http://133.242.159.37/";
            model.UserName = "013045";
            model.Password = "Jwbuxgy9192!";
            model.ConcurrencyMax = 5;
            var manager = new RedmineManager(model);
            return manager;
        }
    }
}