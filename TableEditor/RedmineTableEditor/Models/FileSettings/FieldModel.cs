﻿using LibRedminePower.Converters;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Views.Controls;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.Bases;
using RedmineTableEditor.Models.MyTicketFields.Bases;
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
        public MyIssuePropertyType? MyField { get; set; }
        public int CustomFieldId { get; set; }

        [Obsolete("For Serialize", true)]
        public FieldModel(){}

        public FieldModel(IssuePropertyType field)
        {
            Field = field;
        }

        public FieldModel(MyIssuePropertyType myField)
        {
            MyField = myField;
        }

        public FieldModel(int cfId)
        {
            CustomFieldId = cfId;
        }

        public bool IsType(IssuePropertyType field)
        {
            return Field.HasValue && Field == field;
        }

        public bool IsType(MyIssuePropertyType myField)
        {
            return MyField.HasValue && MyField == myField;
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldModel other)
            {
                if (other.Field.HasValue && Field.HasValue &&
                    other.Field.Value == Field.Value)
                    return true;
                else if (other.MyField.HasValue && MyField.HasValue &&
                    other.MyField.Value == MyField.Value)
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
            hashCode = hashCode * -1521134295 + MyField.GetHashCode();
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
                            CellTemplate = getDataTemplate(prop, bindingBase),
                            IsCellMergingEnabled = prop == IssuePropertyType.Subject,
                        };
                    case IssuePropertyType.SpentHours:
                        return new GridViewDataColumn()
                        {
                            Header = prop.GetDescription(),
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + getPropertyName(prop) + ".Value"),
                            TextAlignment = prop.ToFieldFormat().GetTextAlignment(),
                            IsReadOnly = true,
                            DataFormatString = prop.ToFieldFormat().GetDataFormatString(),
                            CellStyle = getCellStyle(getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplate = getDataTemplate(prop, bindingBase),
                            IsCellMergingEnabled = false,
                        };
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
                            CellStyle = getCellStyle(getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplate = getDataTemplate(prop, bindingBase),
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
                    case IssuePropertyType.Project:
                    case IssuePropertyType.Author:
                    case IssuePropertyType.LastUpdater:
                    case IssuePropertyType.Created:
                    case IssuePropertyType.Updated:
                        return new GridViewDataColumn()
                        {
                            Header = prop.GetDescription(),
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + getPropertyName(prop)),
                            IsReadOnly = true,
                            TextAlignment = prop.ToFieldFormat().GetTextAlignment(),
                            DataFormatString = prop.ToFieldFormat().GetDataFormatString(),
                            CellStyle = getCellStyle(getAutoBackColorPropertyStatus(bindingBase)),
                            IsCellMergingEnabled = prop == IssuePropertyType.Project,
                        };
                    default:
                        throw new NotSupportedException($"prop が {prop} は、サポート対象外です。");
                }
            }
            else if (MyField.HasValue)
            {
                var prop = MyField.Value;
                switch (prop)
                {
                    case MyIssuePropertyType.MySpentHours:
                    case MyIssuePropertyType.DiffEstimatedSpent:
                    case MyIssuePropertyType.ReplyCount:
                    case MyIssuePropertyType.RequiredDays:
                    case MyIssuePropertyType.DaysUntilCreated:
                        return new GridViewDataColumn()
                        {
                            Header = new TextBlock() { Text = prop.GetDescription(), ToolTip = prop.GetToolTip() },
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            DataMemberBinding = new Binding(bindingBase + getPropertyName(prop) + ".Value"),
                            TextAlignment = prop.ToFieldFormat().GetTextAlignment(),
                            IsReadOnly = true,
                            DataFormatString = prop.ToFieldFormat().GetDataFormatString(),
                            CellStyle = getCellStyle(getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplate = getDataTemplate(prop, bindingBase),
                            IsCellMergingEnabled = false,
                        };
                    default:
                        throw new NotSupportedException($"prop が {prop} は、サポート対象外です。");
                }
            }
            else
            {
                var cf = redmine.Cache.CustomFields.SingleOrDefault(a => a.IsEnabled() && a.Id == CustomFieldId);
                if (cf == null) return null;

                var fieldFormat = cf.ToFieldFormat();
                var prop = $"{getCfDicName(fieldFormat)}[{cf.Id}]";
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
                            IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase, cf),
                            DataFormatString = fieldFormat.GetDataFormatString(),
                            CellStyle = getCellStyle(
                                getForegroundPropertyIsEdited(bindingBase + prop + $".{nameof(FieldBase.IsEdited)}"),
                                getAutoBackColorPropertyStatus(bindingBase)),
                            CellTemplateSelector = new CfCellTemplateSelector(bindingBase, cf),
                            IsCellMergingEnabled = false,
                        };
                    case FieldFormat.@bool:
                        return new GridViewDataColumn
                        {
                            Header = cf.Name,
                            Tag = key,
                            ColumnGroupName = key.HasValue ? key.Value.ToString() : null,
                            TextAlignment = fieldFormat.GetTextAlignment(),
                            IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase, cf),
                            DataFormatString = fieldFormat.GetDataFormatString(),
                            CellTemplateSelector = new CfCellTemplateSelector(bindingBase, cf),
                            CellStyle = getCellStyle(getAutoBackColorPropertyStatus(bindingBase)),
                            IsCellMergingEnabled = false,
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
                                IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase, cf),
                                CellTemplateSelector = new CfCellTemplateSelector(bindingBase, cf),
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
                                IsReadOnlyBinding = getIsReadOnlyBinding(bindingBase, cf),
                                CellTemplateSelector = new CfCellTemplateSelector(bindingBase, cf),
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
                        throw new NotSupportedException($"fieldFormat が {fieldFormat} は、サポート対象外です。");
                }
            }
        }

        private (IEnumerable Items, string DisplayPath, string SelectedPath) getCfItemSource(RedmineManager r, CustomField cf)
        {
            var fieldFormat = cf.ToFieldFormat();
            switch (fieldFormat)
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
                    throw new NotSupportedException($"fieldFormat が {fieldFormat} は、サポート対象外です。");
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
                case IssuePropertyType.TotalSpentHours:     return nameof(MyIssueBase.TotalSpentHours);
                case IssuePropertyType.TotalEstimatedHours: return nameof(MyIssueBase.TotalEstimatedHours);
                case IssuePropertyType.Author:              return nameof(MyIssueBase.Author);
                case IssuePropertyType.Updated:             return nameof(MyIssueBase.Updated);
                case IssuePropertyType.Created:             return nameof(MyIssueBase.Created);
                case IssuePropertyType.Project:             return nameof(MyIssueBase.Project);
                case IssuePropertyType.LastUpdater:         return nameof(MyIssueBase.LastUpdater);
                default:
                    throw new NotSupportedException($"prop が {prop} は、サポート対象外です。");
            }
        }

        private string getPropertyName(MyIssuePropertyType prop)
        {
            switch (prop)
            {
                case MyIssuePropertyType.MySpentHours:       return nameof(MyIssueBase.MySpentHours);
                case MyIssuePropertyType.DiffEstimatedSpent: return nameof(MyIssueBase.DiffEstimatedSpent);
                case MyIssuePropertyType.ReplyCount:         return nameof(MyIssueBase.ReplyCount);
                case MyIssuePropertyType.RequiredDays:       return nameof(MyIssueBase.RequiredDays);
                case MyIssuePropertyType.DaysUntilCreated:   return nameof(MyIssueBase.DaysUntilCreated);
                default:
                    throw new NotSupportedException($"prop が {prop} は、サポート対象外です。");
            }
        }

        private string getCfDicName(FieldFormat fieldFormat)
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

        private static IsEditedToRedColorConverter IS_EDITED_TO_RED { get; } = new IsEditedToRedColorConverter();
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

        private static ColumnIsReadOnlyConverter IS_READONLY { get; } = new ColumnIsReadOnlyConverter();
        private Binding getIsReadOnlyBinding(string bindingBase, CustomField cf = null)
        {
            return new Binding(bindingBase) { Converter = IS_READONLY, ConverterParameter = cf };
        }

        protected DataTemplate getDataTemplate(IssuePropertyType prop, string bindingBase)
        {
            switch (prop)
            {
                case IssuePropertyType.EstimatedHours:
                case IssuePropertyType.SpentHours:
                    return createProgressTemplate(bindingBase, getPropertyName(prop), prop.ToFieldFormat().GetDataFormatString());
                default:
                    return null;
            }
        }

        protected DataTemplate getDataTemplate(MyIssuePropertyType prop, string bindingBase)
        {
            switch (prop)
            {
                case MyIssuePropertyType.MySpentHours:
                case MyIssuePropertyType.DiffEstimatedSpent:
                    return createProgressTemplate(bindingBase, getPropertyName(prop), prop.ToFieldFormat().GetDataFormatString());
                case MyIssuePropertyType.ReplyCount:
                case MyIssuePropertyType.RequiredDays:
                case MyIssuePropertyType.DaysUntilCreated:
                    var template = new DataTemplate();
                    var textBlock = new FrameworkElementFactory(typeof(TextBlock));
                    textBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
                    var path = bindingBase + getPropertyName(prop);
                    textBlock.SetBinding(TextBlock.TextProperty, new Binding($"{path}.{nameof(MyTicketFieldBase<int?>.Value)}")
                    {
                        StringFormat = prop.ToFieldFormat().GetDataFormatString(), TargetNullValue = "----"
                    });
                    textBlock.SetBinding(TextBlock.FontStyleProperty, new Binding($"{path}.{nameof(MyTicketFieldBase<int?>.FontStyle)}"));

                    textBlock.SetBinding(RadToolTipService.ToolTipContentProperty, new Binding($"{path}.{nameof(MyTicketFieldBase<int?>.ToolTip)}"));
                    var tooltip = new FrameworkElementFactory(typeof(TextBlock));
                    tooltip.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);
                    tooltip.SetValue(TextBlock.FontStyleProperty, FontStyles.Normal);
                    tooltip.SetBinding(TextBlock.TextProperty, new Binding());
                    var tooltipView = new FrameworkElementFactory(typeof(RadToolTipContentView));
                    tooltipView.AppendChild(tooltip);
                    var tooltipTmp = new DataTemplate();
                    tooltipTmp.VisualTree = tooltipView;
                    tooltipTmp.Seal();
                    textBlock.SetValue(RadToolTipService.ToolTipContentTemplateProperty, tooltipTmp);

                    template.VisualTree = textBlock;
                    template.Seal();
                    return template;
                default:
                    return null;
            }
        }

        private static NullToVisibilityConverter NULL_TO_VISIBLE = new NullToVisibilityConverter();
        private DataTemplate createProgressTemplate(string bindingBase, string propName, string stringFormat)
        {
            // 子チケットの場合のみ設定する
            if (string.IsNullOrEmpty(bindingBase))
                return null;

            var template = new DataTemplate();
            var grid = new FrameworkElementFactory(typeof(Grid));

            var path = bindingBase + propName;
            var progressBar = new FrameworkElementFactory(typeof(ProgressBar));
            progressBar.SetBinding(ProgressBar.ValueProperty, new Binding($"{path}.Value")
                { Mode = BindingMode.OneWay, TargetNullValue = (double)0, FallbackValue = (double)0 });
            progressBar.SetBinding(ProgressBar.MaximumProperty, new Binding($"{path}.Max"));
            progressBar.SetBinding(ProgressBar.VisibilityProperty, new Binding($"{path}.Value")
                { Converter = NULL_TO_VISIBLE, TargetNullValue = Visibility.Collapsed, FallbackValue = Visibility.Collapsed });
            grid.AppendChild(progressBar);

            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            textBlock.SetBinding(TextBlock.TextProperty, new Binding($"{path}.Value") { StringFormat = stringFormat });
            grid.AppendChild(textBlock);

            template.VisualTree = grid;
            template.Seal();
            return template;
        }
    }

    public class CfCellTemplateSelector : DataTemplateSelector
    {
        private string bindingBase { get; set; }
        private CustomField cf { get; set; }

        public CfCellTemplateSelector(string bindingBase, CustomField cf) : base()
        {
            this.bindingBase = bindingBase;
            this.cf = cf;
        }

        private static IsEditedToRedColorConverter IS_EDITED_TO_RED = new IsEditedToRedColorConverter();
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var issue = item as MyIssueBase;
            if (issue == null)
                return null;

            if (!issue.IsEnabledCustomField(cf))
            {
                var textBlock = new FrameworkElementFactory(typeof(TextBlock));
                textBlock.SetValue(TextBlock.TextProperty, "----");
                textBlock.SetValue(TextBlock.ToolTipProperty, string.Format(Properties.Resources.CellErrInvalidField, cf.Name));
                var template = new DataTemplate();
                template.VisualTree = textBlock;
                template.Seal();
                return template;
            }

            var format = cf.ToFieldFormat();
            var cfDicPath = bindingBase + $"{format.GetPropertyName()}[{cf.Id}]";
            switch (format)
            {
                case FieldFormat.@bool:
                    var checkBox = new FrameworkElementFactory(typeof(CheckBox));
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, new Binding(cfDicPath + ".Value"));
                    checkBox.SetValue(CheckBox.IsThreeStateProperty, !cf.IsRequired);
                    checkBox.SetBinding(CheckBox.BorderBrushProperty,
                        new Binding(cfDicPath + $".{nameof(FieldBase.IsEdited)}") { Converter = IS_EDITED_TO_RED });
                    var template = new DataTemplate();
                    template.VisualTree = checkBox;
                    template.Seal();
                    return template;
                default:
                    return null;
            }
        }
    }
}
