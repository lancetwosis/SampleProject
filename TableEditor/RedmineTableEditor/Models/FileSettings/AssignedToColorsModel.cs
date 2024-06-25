using LibRedminePower.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings
{
    public class AssignedToColorsModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public bool IsEnabled { get; set; }
        public bool IsEnabledClosed { get; set; }
        public ObservableCollection<AssignedToColorModel> Items { get; set; } = new ObservableCollection<AssignedToColorModel>();
    }
}
