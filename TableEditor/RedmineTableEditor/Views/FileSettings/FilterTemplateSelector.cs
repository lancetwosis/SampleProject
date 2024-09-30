using RedmineTableEditor.ViewModels.FileSettings.Filters;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Custom;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RedmineTableEditor.Views.FileSettings
{
    public class FilterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemsFilterTemplate { get; set; }
        public DataTemplate TextFilterTemplate { get; set; }
        public DataTemplate IssueFilterTemplate { get; set; }
        public DataTemplate DateFilterTemplate { get; set; }
        public DataTemplate NumericFilterTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case ProjectFilterViewModel p1:
                case TrackerFilterViewModel t:
                case StatusFilterViewModel s:
                case PriorityFilterViewModel p2:
                case VersionFilterViewModel v1:
                case IssueCategoryFilterViewModel c:
                case AssigneeFilterViewModel a1:
                case AuthorFilterViewModel a2:
                case UpdaterFilterViewModel u1:
                case LastUpdaterFilterViewModel l:
                case CfItemsFilterViewModel i:
                case CfUserFilterViewModel u2:
                case CfVersionFilterViewModel v2:
                case CfBoolFilterViewModel b:
                    return ItemsFilterTemplate;
                case SubjectFilterViewModel s:
                case DescriptionFilterViewModel d:
                case CommentFilterViewModel c:
                case CfTextSearchFilterViewModel t:
                    return TextFilterTemplate;
                case ParentIssueFilterViewModel p:
                case ChildIssueFilterViewModel c:
                    return IssueFilterTemplate;
                case CreatedOnFilterViewModel c:
                case UpdatedOnFilterViewModel u:
                case StartDateFilterViewModel s:
                case DueDateFilterViewModel d1:
                case CfDateFilterViewModel d2:
                    return DateFilterTemplate;
                case CfIntegerFilterViewModel i:
                case CfFloatFilterViewModel f:
                    return NumericFilterTemplate;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
