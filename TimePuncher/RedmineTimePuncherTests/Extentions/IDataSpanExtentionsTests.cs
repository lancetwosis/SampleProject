using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Extentions.Tests
{
    [TestClass()]
    public class IDataSpanExtentionsTests
    {
        [TestMethod()]
        public void IntersectsWithTest()
        {
            var apo = new MyAppointment();
            apo.Start = new DateTime(2022, 4, 24, 9, 0, 0);
            apo.End = new DateTime(2022, 4, 24, 10, 0, 0);

            var term1 = new TermModel(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            Assert.AreEqual(true, apo.IntersectsWith(term1));

            var term2 = new TermModel(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));
            Assert.AreEqual(true, apo.IntersectsWith(term2));

            var term3 = new TermModel(new TimeSpan(8, 0, 0), new TimeSpan(11, 0, 0));
            Assert.AreEqual(true, apo.IntersectsWith(term3));

            var term4 = new TermModel(new TimeSpan(9, 30, 0), new TimeSpan(11, 0, 0));
            Assert.AreEqual(true, apo.IntersectsWith(term4));

            var term5 = new TermModel(new TimeSpan(9, 30, 0), new TimeSpan(9, 45, 0));
            Assert.AreEqual(true, apo.IntersectsWith(term5));

            var term6 = new TermModel(new TimeSpan(8, 00, 0), new TimeSpan(8, 45, 0));
            Assert.AreEqual(false, apo.IntersectsWith(term6));

            var term7 = new TermModel(new TimeSpan(8, 00, 0), new TimeSpan(9, 0, 0));
            Assert.AreEqual(false, apo.IntersectsWith(term7));

            var term8 = new TermModel(new TimeSpan(10, 00, 0), new TimeSpan(10, 0, 0));
            Assert.AreEqual(false, apo.IntersectsWith(term8));
        }
    }
}