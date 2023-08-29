using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings
{
    public class FileSettingsModel : LibRedminePower.Models.Bases.ModelBase
    {
        public ParentIssueSettingsModel ParentIssues { get; set; }
        public SubIssueSettingsModel SubIssues { get; set; }
        public AutoBackColorModel AutoBackColor { get; set; }

        public FileSettingsModel()
        {
            ParentIssues = new ParentIssueSettingsModel();
            SubIssues = new SubIssueSettingsModel();
            AutoBackColor = new AutoBackColorModel();
        }
    }
}
