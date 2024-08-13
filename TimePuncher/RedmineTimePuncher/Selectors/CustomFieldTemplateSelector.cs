using RedmineTimePuncher.ViewModels;
using RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Fields;
using RedmineTimePuncher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;

namespace RedmineTimePuncher.Selectors
{
    public class CustomFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate { get; set; }
        public DataTemplate LongTextBoxTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }
        public DataTemplate MultiComboBoxTemplate { get; set; }
        public DataTemplate IdNameComboBoxTemplate { get; set; }
        public DataTemplate IdNameMultiComboBoxTemplate { get; set; }
        public DataTemplate DateTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case StringCustomFieldViewModel s:
                case IntCustomFieldViewModel i:
                case FloatCustomFieldViewModel f:
                    return TextBoxTemplate;
                case ListCustomFieldViewModel l:
                    return l.CustomField.Multiple ? MultiComboBoxTemplate : ComboBoxTemplate;
                case UserCustomFieldViewModel u:
                    return u.CustomField.Multiple ? IdNameMultiComboBoxTemplate : IdNameComboBoxTemplate;
                case VersionCustomFieldViewModel v:
                    return v.CustomField.Multiple ? IdNameMultiComboBoxTemplate : IdNameComboBoxTemplate;
                case BoolCustomFieldViewModel b:
                    return IdNameComboBoxTemplate;
                case TextCustomFieldViewModel t:
                    return LongTextBoxTemplate;
                case DateCustomFieldViewModel d:
                    return DateTemplate;
                case KeyValueCustomFieldViewModel k:
                    return k.CustomField.Multiple ? IdNameMultiComboBoxTemplate : IdNameComboBoxTemplate;
                default:
                    return null;
            }
        }
    }
}
