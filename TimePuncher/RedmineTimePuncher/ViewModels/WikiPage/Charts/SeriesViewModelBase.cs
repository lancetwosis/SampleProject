using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.WikiPage.Charts
{
    public abstract class SeriesViewModelBase
    {
        public string Title { get; set; }
        public List<DataItem> Items { get; set; }
    }
}
