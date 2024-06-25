using LibRedminePower.Converters;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Views.Controls;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.Bases;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Standard;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace RedmineTableEditor.Models.FileSettings
{
    public class FieldModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public IssuePropertyType? Field { get; set; }
        public int CustomFieldId { get; set; }

        [Obsolete("For Serialize", true)]
        public FieldModel(){}

        public FieldModel(IssuePropertyType field)
        {
            Field = field;
        }

        public FieldModel(int cfId)
        {
            CustomFieldId = cfId;
        }

        public bool IsDetail()
        {
            return Field.HasValue ? Field.Value.IsDetail() : false;
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldModel other)
            {
                if (other.Field.HasValue && Field.HasValue &&
                    other.Field.Value == Field.Value)
                    return true;
                else if (other.CustomFieldId != 0 && CustomFieldId != 0 &&
                         other.CustomFieldId == CustomFieldId)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 743879;
            hashCode = hashCode * -1521134295 + Field.GetHashCode();
            hashCode = hashCode * -1521134295 + CustomFieldId.GetHashCode();
            return hashCode;
        }

        public GridViewBoundColumnBase CreateColumn(RedmineManager redmine, int? key = null)
        {
            var bindingBase = key.HasValue ? $"{nameof(MyIssue.ChildrenDic)}[{key}]." : "";

            if (Field.HasValue)
            {
                var prop = Field.Value;
                switch (prop)
                {
                    case IssuePropertyType.Id:
                        return new GridViewHyperlinkColumn()
                        {
                            Header = prop.GetDescription(),
                            Tag = key,
                            IsReadOnly = true,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + $"{nameof(MyIssueBase.Url)}"),
                            ContentBinding = new Binding(bindingBase + getPropertyName(prop)),
                            TextAlignment = TextAlignment.Left,
                            CellStyle = getCellStyle(getAutoBackColorPropertyStatus(bindingBase)),
                        };
                    case IssuePropertyType.Subject:
                    case IssuePropertyType.StartDate:
                    case IssuePropertyType.DueDate:
                    case IssuePropertyType.DoneRatio:
                    case IssuePropertyType.EstimatedHours:
                        return new GridViewDataColumn()
                        {
                            Header = prop.GetDescription(),
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + getPropertyName(prop) + ".Value"),
                            IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase),
                            TextAlignment = prop.ToFieldFormat().GetTextAlignment(),
                            DataFormatString = prop.ToFieldFormat().GetDataFormatString(),
                            CellStyle = getCellStyle(
                                getForegroundPropertyIsEdited(bindingBase + getPropertyName(prop) + $".{nameof(FieldBase.IsEdited)}"),
                                getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplate = getDataTemplate(prop, key),
                            IsCellMergingEnabled = prop == IssuePropertyType.Subject,
                        };
                    case IssuePropertyType.SpentHours:
                    case IssuePropertyType.MySpentHours:
                    case IssuePropertyType.DiffEstimatedSpent:
                    case IssuePropertyType.TotalSpentHours:
                    case IssuePropertyType.TotalEstimatedHours:
                        return new GridViewDataColumn()
                        {
                            Header = prop.GetDescription(),
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + getPropertyName(prop)),
                            TextAlignment = prop.ToFieldFormat().GetTextAlignment(),
                            IsReadOnly = true,
                            DataFormatString = prop.ToFieldFormat().GetDataFormatString(),
                            CellStyle = getCellStyle(
                                getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplate = getDataTemplate(prop, key),
                            IsCellMergingEnabled = false,
                        };
                    case IssuePropertyType.Tracker:
                    case IssuePropertyType.Status:
                    case IssuePropertyType.AssignedTo:
                    case IssuePropertyType.FixedVersion:
                    case IssuePropertyType.Category:
                    case IssuePropertyType.Priority:
                        var newStyle = (Style)Application.Current.FindResource("girdViewComboBoxEditStyle");
                        return new GridViewComboBoxColumn()
                        {
                            Header = prop.GetDescription(),
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            TextAlignment = prop.ToFieldFormat().GetTextAlignment(),
                            DataMemberBinding = new Binding(bindingBase + getPropertyName(prop) + ".Value"),
                            IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase),
                            CellStyle = getCellStyle(
                                getForegroundPropertyIsEdited(bindingBase + getPropertyName(prop) + $".{nameof(FieldBase.IsEdited)}"),
                                getAutoBackColorPropertyStatus(bindingBase)),
                            ItemsSource = prop.GetPropertyItemSource(redmine),
                            DisplayMemberPath = "Name",
                            SelectedValueMemberPath = "Id",
                            IsComboBoxEditable = true,
                            IsLightweightModeEnabled = true,
                            EditorStyle = newStyle,
                            IsFilterable = true,
                        };
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                var cf = redmine.CustomFields.SingleOrDefault(a => a.Id == CustomFieldId);
                if (cf == null) return null;

                var fieldFormat = cf.ToFieldFormat();
                var prop = $"{getCfPropName(fieldFormat)}[{cf.Id}]";
                switch (fieldFormat)
                {
                    case FieldFormat.@string:
                    case FieldFormat.text:
                    case FieldFormat.link:
                    case FieldFormat.@int:
                    case FieldFormat.@float:
                    case FieldFormat.date:
                        return new GridViewDataColumn()
                        {
                            Header = cf.Name,
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            TextAlignment = fieldFormat.GetTextAlignment(),
                            DataMemberBinding = new Binding(bindingBase + prop + ".Value"),
                            IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase),
                            DataFormatString = fieldFormat.GetDataFormatString(),
                            CellStyle = getCellStyle(
                                getForegroundPropertyIsEdited(bindingBase + prop + $".{nameof(FieldBase.IsEdited)}"),
                                getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplate = null,
                            IsCellMergingEnabled = false,
                        };
                    case FieldFormat.@bool:
                        return new GridViewCheckBoxColumn
                        {
                            Header = cf.Name,
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + prop + ".Value"),
                            TextAlignment = fieldFormat.GetTextAlignment(),
                            IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase),
                            DataFormatString = fieldFormat.GetDataFormatString(),
                            CellStyle = getCellStyle(
                                getForegroundPropertyIsEdited(bindingBase + prop + $".{nameof(FieldBase.IsEdited)}"),
                                getAutoBackColorPropertyStatus(bindingBase)),
                        };
                    case FieldFormat.user:
                    case FieldFormat.version:
                    case FieldFormat.list:
                    case FieldFormat.enumeration:
                        {
                            var temp = getCfItemSource(redmine, cf);
                            return new GridViewComboBoxColumn()
                            {
                                Header = cf.Name,
                                Tag = key,
                                ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                                DataMemberBinding = new Binding(bindingBase + prop + ".Value"),
                                TextAlignment = fieldFormat.GetTextAlignment(),
                                IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase),
                                CellStyle = getCellStyle(
                                    getForegroundPropertyIsEdited(bindingBase + prop + $".{nameof(FieldBase.IsEdited)}"),
                                    getAutoBackColorPropertyStatus(bindingBase)),
                                ItemsSource = temp.Items,
                                DisplayMemberPath = temp.DisplayPath,
                                SelectedValueMemberPath = temp.SelectedPath,
                                IsComboBoxEditable = true,
                                IsLightweightModeEnabled = true,
                                IsFilterable = true,
                            };
                        }
                    case FieldFormat.list_multi:
                    case FieldFormat.enumeration_multi:
                    case FieldFormat.version_multi:
                    case FieldFormat.user_multi:
                        {
                            var temp = getCfItemSource(redmine, cf);
                            return new MultiSelectionGridViewComboBoxColumn()
                            {
                                Header = cf.Name,
                                Tag = key,
                                ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                                DataMemberBinding = new Binding(bindingBase + prop + ".Value"),
                                TextAlignment = fieldFormat.GetTextAlignment(),
                                IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase),
                                CellStyle = getCellStyle(
                                    getForegroundPropertyIsEdited(bindingBase + prop + $".{nameof(FieldBase.IsEdited)}"),
                                    getAutoBackColorPropertyStatus(bindingBase)),
                                ItemsSource = temp.Items,
                                DisplayMemberPath = temp.DisplayPath,
                                SelectedValuePath = temp.SelectedPath,
                                IsFilterable = true,
                                IsCellMergingEnabled = false,
                            };
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private (IEnumerable Items, string DisplayPath, string SelectedPath) getCfItemSource(RedmineManager r, CustomField cf)
        {
            switch (cf.ToFieldFormat())
            {
                case FieldFormat.user:
                case FieldFormat.user_multi:
                    return (r.Users, nameof(IdentifiableName.Name), nameof(IdentifiableName.Id));
                case FieldFormat.version:
                case FieldFormat.version_multi:
                    return (r.Versions, nameof(Redmine.Net.Api.Types.Version.Name), nameof(Redmine.Net.Api.Types.Version.Id));
                case FieldFormat.list:
                case FieldFormat.list_multi:
                case FieldFormat.enumeration:
                case FieldFormat.enumeration_multi:
                    return (cf.PossibleValues, nameof(CustomFieldPossibleValue.Label), nameof(CustomFieldPossibleValue.Value));
                default:
                    throw new InvalidOperationException();
            }
        }

        private string getPropertyMax(IssuePropertyType prop)
        {
            switch (prop)
            {
                case IssuePropertyType.EstimatedHours:     return nameof(MyIssueBase.EstimatedHoursMax);
                case IssuePropertyType.SpentHours:         return nameof(MyIssueBase.SpentHoursMax);
                case IssuePropertyType.MySpentHours:       return nameof(MyIssueBase.MySpentHoursMax);
                case IssuePropertyType.DiffEstimatedSpent: return nameof(MyIssueBase.DiffEstimatedSpentMax);
                case IssuePropertyType.Status:
                case IssuePropertyType.AssignedTo:
                case IssuePropertyType.FixedVersion:
                case IssuePropertyType.Category:
                case IssuePropertyType.Priority:
                case IssuePropertyType.Id:
                case IssuePropertyType.Subject:
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours:
                default:
                    throw new InvalidOperationException();
            }
        }

        private string getPropertyName(IssuePropertyType prop)
        {
            switch (prop)
            {
                case IssuePropertyType.Id:                  return nameof(MyIssueBase.IdLabel);
                case IssuePropertyType.Subject:             return nameof(MyIssueBase.Subject);
                case IssuePropertyType.Tracker:             return nameof(MyIssueBase.Tracker);
                case IssuePropertyType.Status:              return nameof(MyIssueBase.Status);
                case IssuePropertyType.AssignedTo:          return nameof(MyIssueBase.AssignedTo);
                case IssuePropertyType.FixedVersion:        return nameof(MyIssueBase.FixedVersion);
                case IssuePropertyType.Priority:            return nameof(MyIssueBase.Priority);
                case IssuePropertyType.Category:            return nameof(MyIssueBase.Category);
                case IssuePropertyType.StartDate:           return nameof(MyIssueBase.StartDate);
                case IssuePropertyType.DueDate:             return nameof(MyIssueBase.DueDate);
                case IssuePropertyType.DoneRatio:           return nameof(MyIssueBase.DoneRatio);
                case IssuePropertyType.EstimatedHours:      return nameof(MyIssueBase.EstimatedHours);
                case IssuePropertyType.SpentHours:          return nameof(MyIssueBase.SpentHours);
                case IssuePropertyType.MySpentHours:        return nameof(MyIssueBase.MySpentHours);
                case IssuePropertyType.DiffEstimatedSpent:  return nameof(MyIssueBase.DiffEstimatedSpent);
                case IssuePropertyType.TotalSpentHours:     return nameof(MyIssueBase.TotalSpentHours);
                case IssuePropertyType.TotalEstimatedHours: return nameof(MyIssueBase.TotalEstimatedHours);
                default:
                    throw new InvalidOperationException();
            }
        }

        private string getCfPropName(FieldFormat fieldFormat)
        {
            switch (fieldFormat)
            {
                case FieldFormat.@string:
                case FieldFormat.text:
                case FieldFormat.link:
                case FieldFormat.list:              return nameof(MyIssueBase.DicCustomFieldString);
                case FieldFormat.@float:            return nameof(MyIssueBase.DicCustomFieldFloat);
                case FieldFormat.@bool:             return nameof(MyIssueBase.DicCustomFieldBool);
                case FieldFormat.@int:
                case FieldFormat.user:
                case FieldFormat.version:
                case FieldFormat.enumeration:       return nameof(MyIssueBase.DicCustomFieldInt);
                case FieldFormat.date:              return nameof(MyIssueBase.DicCustomFieldDate);
                case FieldFormat.version_multi:
                case FieldFormat.user_multi:        return nameof(MyIssueBase.DicCustomFieldInts);
                case FieldFormat.list_multi:
                case FieldFormat.enumeration_multi: return nameof(MyIssueBase.DicCustomFieldStrings);
                default:
                    return null;
            }
        }

        protected Style getCellStyle(params Setter[] setters)
        {
            var cellStyleBase = (Style)Application.Current.FindResource(typeof(GridViewCell));
            var cellStyle = new Style(typeof(GridViewCell)) { BasedOn = cellStyleBase };
            setters.ToList().ForEach(a => cellStyle.Setters.Add(a));

            return cellStyle;
        }

        private static IsEditedToRedColorConverter IS_EDITED_TO_RED = new IsEditedToRedColorConverter();
        protected Setter getForegroundPropertyIsEdited(string propertyName)
        {
            return new Setter(GridViewCell.ForegroundProperty, new Binding(propertyName)
            {
                Converter = IS_EDITED_TO_RED,
            });
        }

        private static AutoCellBackColorConverter AUTO_BACK_COLOR = new AutoCellBackColorConverter();
        protected Setter getAutoBackColorPropertyStatus(string bindingBase)
        {
            var multiBinding = new MultiBinding();
            multiBinding.Converter = AUTO_BACK_COLOR;
            multiBinding.Bindings.Add(new Binding($"DataContext.{nameof(TableEditorViewModel.Issues)}.{nameof(IssuesViewModel.AutoBackColor)}")
            {
                RelativeSource = new RelativeSource()
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = typeof(TableEditorView)
                }
            });
            multiBinding.Bindings.Add(new Binding($"DataContext.{nameof(TableEditorViewModel.Issues)}.{nameof(IssuesViewModel.Redmine)}.Value")
            {
                RelativeSource = new RelativeSource()
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = typeof(TableEditorView)
                }
            });
            multiBinding.Bindings.Add(new Binding(bindingBase + nameof(Status) + ".Value"));
            multiBinding.Bindings.Add(new Binding(bindingBase + nameof(AssignedTo) + ".DisplayValue"));
            return new Setter(GridViewCell.BackgroundProperty, multiBinding);
        }

        private static IsNullConverter IS_NULL = new IsNullConverter();
        private Binding getIsReadOnlyBinding(string bindingBase)
        {
            var binding = new Binding(bindingBase + nameof(MyIssueBase.Issue));
            binding.Converter = IS_NULL;
            return binding;
        }

        private static NullToVisibilityConverter NULL_TO_VISIBLE = new NullToVisibilityConverter();
        protected DataTemplate getDataTemplate(IssuePropertyType prop, int? key)
        {
            if (!key.HasValue) return null;

            var bindingBase = $"{nameof(MyIssue.ChildrenDic)}[{key}].";
            switch (prop)
            {
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
                case IssuePropertyType.MySpentHours:
                case IssuePropertyType.DiffEstimatedSpent:
                    {
                        var template = new DataTemplate();
                        var grid = new FrameworkElementFactory(typeof(Grid));

                        var propName = getPropertyName(prop);
                        var propInfo = typeof(MyIssueBase).GetProperty(propName);
                        var valuePath = bindingBase + propName;
                        if (propInfo.CanWrite) valuePath += ".Value";

                        var maxPropInfo = typeof(MyIssueBase).GetProperty(getPropertyMax(prop));
                        var maxPropPath = new PropertyPath("(0)", maxPropInfo);
                        var progressBar = new FrameworkElementFactory(typeof(ProgressBar));
                        progressBar.SetBinding(ProgressBar.ValueProperty, new Binding(valuePath) { Mode = BindingMode.OneWay, TargetNullValue = (double)0, FallbackValue = (double)0 });
                        progressBar.SetBinding(ProgressBar.MaximumProperty, new Binding() { Path = maxPropPath });
                        progressBar.SetBinding(ProgressBar.VisibilityProperty, new Binding(valuePath) { Converter = NULL_TO_VISIBLE, TargetNullValue = Visibility.Collapsed, FallbackValue = Visibility.Collapsed });
                        grid.AppendChild(progressBar);

                        var textBlock = new FrameworkElementFactory(typeof(TextBlock));
                        textBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
                        textBlock.SetBinding(TextBlock.TextProperty, new Binding(valuePath) { StringFormat = prop.ToFieldFormat().GetDataFormatString() });
                        grid.AppendChild(textBlock);

                        template.VisualTree = grid;
                        template.Seal();
                        return template;
                    }
                case IssuePropertyType.StartDate:
                case IssuePropertyType.DueDate:
                case IssuePropertyType.Id:
                case IssuePropertyType.Subject:
                case IssuePropertyType.Status:
                case IssuePropertyType.AssignedTo:
                case IssuePropertyType.FixedVersion:
                case IssuePropertyType.Priority:
                case IssuePropertyType.Category:
                case IssuePropertyType.DoneRatio:
                case IssuePropertyType.TotalSpentHours:
                case IssuePropertyType.TotalEstimatedHours:
                default:
                    return null;
            }
        }

    }
}
