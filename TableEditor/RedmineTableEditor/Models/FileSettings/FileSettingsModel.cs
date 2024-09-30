using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings
{
    public class FileSettingsModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        // デフォルトでは Id, Subject までを固定表示とする
        public static int DEFAULT_FROZEN_COUNT = 2;
        public int FrozenColumnCount { get; set; } = DEFAULT_FROZEN_COUNT;

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
