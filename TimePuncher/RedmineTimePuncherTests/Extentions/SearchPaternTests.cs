using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Extentions.Tests
{
    [TestClass()]
    public class SearchPaternTests
    {
        [TestMethod()]
        public void CheckTest()
        {
            Assert.IsTrue(SearchPatern.Check("abc", "1abcd"));
            Assert.IsTrue(SearchPatern.Check("ab c", "1abcd"));
            Assert.IsTrue(SearchPatern.Check("ab +c", "1abcd"));
            Assert.IsFalse(SearchPatern.Check("ab -c", "1abcd"));
            Assert.IsTrue(SearchPatern.Check("\"ab c\"", "1ab cd"));
            Assert.IsTrue(SearchPatern.Check("\"-> 終了\"", "abc (新規 -> 終了) "));
            Assert.IsFalse(SearchPatern.Check("-\"-> 終了\"", "abc (新規 -> 終了) "));
        }
    }
}