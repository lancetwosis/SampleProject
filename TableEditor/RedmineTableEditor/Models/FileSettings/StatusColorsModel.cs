using LibRedminePower.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings
{
    public class StatusColorsModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public bool IsEnabled { get; set; }
        public ObservableCollection<StatusColorModel> Items { get; set; }

        public StatusColorsModel()
        {
            Items = new ObservableCollection<StatusColorModel>();
        }
    }
}
