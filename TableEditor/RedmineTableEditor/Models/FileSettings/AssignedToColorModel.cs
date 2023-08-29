using System;

namespace RedmineTableEditor.Models.FileSettings
{
    public class AssignedToColorModel : LibRedminePower.Models.Bases.ModelBase
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public System.Drawing.Color Color { get; set; }
    }
}