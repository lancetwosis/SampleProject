using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class ReviewIsRequiredSettingModel : CustomFieldSettingModelBase
    {
        public ReviewIsRequiredSettingModel() : base(CustomFieldFormat.Bool)
        {
            PossibleValues.Add(new MyCustomFieldPossibleValue(Properties.Resources.ReviewReviewerRequired, true, true));
            PossibleValues.Add(new MyCustomFieldPossibleValue(Properties.Resources.ReviewReviewerOptional, false));
            Value = getDefaultValue();
        }

        public override void Update(List<MyCustomField> possibleCustomFields)
        {
            this.updateValue();
            this.updateCustomFields(possibleCustomFields);
        }
    }
}
