using RedmineTableEditor.Models.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels
{
    public class MyIssueEditedListener
    {
        public static Action<MyIssueBase, bool> EditedChanged;
        private static MyIssueEditedListener instance = new MyIssueEditedListener();

        private MyIssueEditedListener() {}

        public static MyIssueEditedListener Instance => instance;

        public event EventHandler Edited;

        public void OnEdited(MyIssueBase myIssue)
        {
            Edited(myIssue, EventArgs.Empty);
        }
    }
}
