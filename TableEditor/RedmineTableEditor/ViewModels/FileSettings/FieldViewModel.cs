using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class FieldViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public Enums.IssuePropertyType? Field { get; set; }
        public CustomField CustomField { get; set; }
        public Models.FileSettings.FieldModel Model { get; set; }

        public FieldViewModel(Models.FileSettings.FieldModel model, CustomField customField)
        {
            Model = model;
            Field = model.Field;
            CustomField = customField;
        }

        public override string ToString()
        {
            if (Field.HasValue)
                return Field.Value.GetDescription();
            else
                return CustomField.Name;
        }
    }
}
