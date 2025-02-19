using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class FieldViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public IssuePropertyType? Field { get; set; }
        public MyIssuePropertyType? MyField { get; set; }

        public CustomField CustomField { get; set; }
        public Models.FileSettings.FieldModel Model { get; set; }

        public string ToolTip
        {
            get
            {
                return MyField.HasValue ? MyField.Value.GetToolTip() : null;
            }
        }

        public FieldViewModel(Models.FileSettings.FieldModel model, CustomField customField)
        {
            Model = model;
            Field = model.Field;
            MyField = model.MyField;
            CustomField = customField;
        }

        public override string ToString()
        {
            if (Field.HasValue)
                return Field.Value.GetDescription();
            else if (MyField.HasValue)
                return MyField.Value.GetDescription();
            else
                return CustomField.Name;
        }
    }
}
