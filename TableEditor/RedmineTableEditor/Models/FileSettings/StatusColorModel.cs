using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTableEditor.Models.FileSettings
{
    public class StatusColorModel : LibRedminePower.Models.Bases.ModelBase
    {
        public int Id { get; set; }
        public System.Drawing.Color Color { get; set; }
    }
}
