using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class ReviewNeedsFaceToFaceSettingModel : CustomFieldSettingModelBase
    {
        public ReviewNeedsFaceToFaceSettingModel() : base(CustomFieldFormat.Bool)
        {
            PossibleValues.Add(new MyCustomFieldPossibleValue(Properties.Resources.ReviewMethodDesktop, false, true));
            PossibleValues.Add(new MyCustomFieldPossibleValue(Properties.Resources.ReviewMethodFaceToFace, true));
            Value = GetDefaultValue();
        }

        public override void Update(List<MyCustomField> possibleCustomFields)
        {
            this.updateValue();
            this.updateCustomFields(possibleCustomFields);
        }
    }
}
