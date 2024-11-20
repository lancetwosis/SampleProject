using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields
{
    public class ReviewIsRequiredSettingModel : CustomFieldSettingModelBase
    {
        public static MyCustomFieldPossibleValue REQUIRED =
            new MyCustomFieldPossibleValue(Properties.Resources.ReviewReviewerRequired, true, true);
        public static MyCustomFieldPossibleValue OPTIONAL =
            new MyCustomFieldPossibleValue(Properties.Resources.ReviewReviewerOptional, false);

        public ReviewIsRequiredSettingModel() : base(CustomFieldFormat.Bool)
        {
        }

        public override List<MyCustomFieldPossibleValue> GetPossibleValues()
        {
            var values = new List<MyCustomFieldPossibleValue>();
            if (IsEnabled)
            {
                values.Add(REQUIRED);
                values.Add(OPTIONAL);
            }

            return values;
        }
    }
}
