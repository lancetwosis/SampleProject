﻿using LibRedminePower.Extentions;
using LibRedminePower.Views.Controls;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Custom;
using RedmineTableEditor.Models.TicketFields.Standard;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Views;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.Bases
{
    public abstract class MyIssueBase : LibRedminePower.Models.Bases.ModelBase
    {
        private static double estimatedHoursMax;
        public static double EstimatedHoursMax 
        { 
            get { return estimatedHoursMax; }
            set
            {
                estimatedHoursMax = value;
                NotifyStaticPropertyChanged();
            }
        }

        private static double spentHoursMax;
        public static double SpentHoursMax
        {
            get { return spentHoursMax; }
            set 
            {
                spentHoursMax = value;
                NotifyStaticPropertyChanged();
            }
        }

        private static double mySpentHoursMax;
        public static double MySpentHoursMax
        {
            get { return mySpentHoursMax; }
            set
            {
                mySpentHoursMax = value;
                NotifyStaticPropertyChanged();
            }
        }

        private static double diffEstimatedSpentMax;
        public static double DiffEstimatedSpentMax
        {
            get { return diffEstimatedSpentMax; }
            set
            {
                diffEstimatedSpentMax = value;
                NotifyStaticPropertyChanged();
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public static void NotifyStaticPropertyChanged([CallerMemberName] string propertyName = "") =>
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

        public Issue Issue { get; set; }
        public int? Id => Issue?.Id;
        public int Depth { get; set; }
        public string IdLabel => Depth == 0 ? $"{Id}" : $"{string.Join("", Enumerable.Range(0, Depth - 1).Select(_ => "  "))}> {Id}";
        public string Url { get; set; }
        public ReactiveProperty<bool> IsEdited { get; set; }

        public Subject Subject { get; set; }
        public TicketFields.Standard.Tracker Tracker { get; set; }
        public Status Status { get; set; }
        public AssignedTo AssignedTo { get; set; }
        public FixedVersion FixedVersion { get; set; }
        public Priority Priority { get; set; }
        public Category Category { get; set; }
        public StartDate StartDate { get; set; }
        public DueDate DueDate { get; set; }
        public DoneRatio DoneRatio { get; set; }
        public EstimatedHours EstimatedHours { get; set; }
        public float? SpentHours => Issue?.SpentHours;
        public float? TotalSpentHours => Issue?.TotalSpentHours;
        public float? TotalEstimatedHours => Issue?.TotalEstimatedHours;
        public double? MySpentHours
        {
            get
            {
                if (Issue == null) return null;
                if (timeEntries == null) return null;
                if (AssignedTo.Value == null) return null;

                var myDeci = timeEntries.Where(a => a.User.Id == AssignedTo.Value.Value).Sum(a => a.Hours);
                var result = decimal.ToDouble(myDeci);
                if (MySpentHoursMax < result)
                    MySpentHoursMax = result;
                return result;
            }
        }

        public double? DiffEstimatedSpent
        {
            get
            {
                // 両方NULLならばNULLにする
                if (!EstimatedHours.Value.HasValue && !MySpentHours.HasValue) return null;

                var esimated = EstimatedHours.Value.HasValue ? EstimatedHours.Value.Value : 0;
                var spent = MySpentHours.HasValue ? MySpentHours.Value : 0;
                var result = esimated - spent;
                if (result <= 0 || Status.SelectedItem.IsClosed) result = 0;
                if (DiffEstimatedSpentMax < result)
                    DiffEstimatedSpentMax = result;
                return result;
            }
        }

        // 当初は、カスタムフィールドのプロパティは、型毎に作らずに、一つのプロパティにまとめるため、Generic型のプロパティを定義していた。
        // Generic型のプロパティの場合、GridView側が型判断が出来ずに、コピペができないことがわかった。
        // よって、多少煩雑であるが型毎にプロパティを用意した。
        public Dictionary<int, CfString> DicCustomFieldString { get; set; }
        public Dictionary<int, CfFloat> DicCustomFieldFloat { get; set; }
        public Dictionary<int, CfBool> DicCustomFieldBool { get; set; }
        public Dictionary<int, CfInt> DicCustomFieldInt { get; set; }
        public Dictionary<int, CfDate> DicCustomFieldDate { get; set; }
        public Dictionary<int, CfInts> DicCustomFieldInts { get; set; }
        public Dictionary<int, CfStrings> DicCustomFieldStrings { get; set; }

        private List<TimeEntry> timeEntries;
        private RedmineManager redmine;
        private bool isSub = false;

        public MyIssueBase(bool isSub, Issue issue, RedmineManager redmine)
        {
            this.isSub = isSub;
            this.redmine = redmine;
            SetIssue(issue);
        }

        public void SetIssue(Issue issue)
        {
            this.Issue = issue;
            this.Url = issue != null ? redmine.GetIssueUrl(issue.Id) : "";

            DicCustomFieldString = new Dictionary<int, CfString>();
            DicCustomFieldFloat = new Dictionary<int, CfFloat>();
            DicCustomFieldBool = new Dictionary<int, CfBool>();
            DicCustomFieldInt = new Dictionary<int, CfInt>();
            DicCustomFieldDate = new Dictionary<int, CfDate>();
            DicCustomFieldInts = new Dictionary<int, CfInts>();
            DicCustomFieldStrings = new Dictionary<int, CfStrings>();

            var editableProps = new ObservableCollection<FieldBase>();
            editableProps.Add(Subject = new Subject(issue));
            editableProps.Add(Tracker = new TicketFields.Standard.Tracker(issue, redmine));
            editableProps.Add(Status = new Status(issue, redmine));
            editableProps.Add(AssignedTo = new AssignedTo(issue, redmine));
            editableProps.Add(FixedVersion = new FixedVersion(issue, redmine));
            editableProps.Add(Category = new Category(issue, redmine));
            editableProps.Add(Priority = new Priority(issue, redmine));
            editableProps.Add(StartDate = new StartDate(issue));
            editableProps.Add(DueDate = new DueDate(issue));
            editableProps.Add(DoneRatio = new DoneRatio(issue));
            editableProps.Add(EstimatedHours = new EstimatedHours(issue));

            if (issue != null && issue.CustomFields != null && issue.CustomFields.Any())
            {
                var fields =issue.CustomFields.Select(a => a.ToFieldBase(redmine.CustomFields.Single(b => b.Id == a.Id))).ToList();
                foreach (var cf in fields)
                {
                    editableProps.Add(cf);
                    switch (cf)
                    {
                        case CfString cfString:
                            DicCustomFieldString.Add(cfString.Cf.Id, cfString);
                            break;
                        case CfFloat cfFloat:
                            DicCustomFieldFloat.Add(cfFloat.Cf.Id, cfFloat);
                            break;
                        case CfBool cfBool:
                            DicCustomFieldBool.Add(cfBool.Cf.Id, cfBool);
                            break;
                        case CfInt cfInt:
                            DicCustomFieldInt.Add(cfInt.Cf.Id, cfInt);
                            break;
                        case CfDate cfDate:
                            DicCustomFieldDate.Add(cfDate.Cf.Id, cfDate);
                            break;
                        case CfInts cfInts:
                            DicCustomFieldInts.Add(cfInts.Cf.Id, cfInts);
                            break;
                        case CfStrings cfStrings:
                            DicCustomFieldStrings.Add(cfStrings.Cf.Id, cfStrings);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            EstimatedHours.ObserveProperty(a => a.Value).Where(a => a.HasValue).Subscribe(v =>
            {
                if (EstimatedHoursMax < v)
                    EstimatedHoursMax = v.Value;
            }).AddTo(disposables);
            this.ObserveProperty(a => a.SpentHours).Where(a => a.HasValue).Subscribe(v =>
            {
                if (SpentHoursMax < v)
                    SpentHoursMax = v.Value;
            }).AddTo(disposables);

            editableProps.ObserveElementProperty(a => a.IsEdited).Subscribe(a =>
            {
                MyIssueEditedListener.EditedChanged?.Invoke(this, a.Value);
            });

            var editedProps = editableProps.ToFilteredReadOnlyObservableCollection(a => a.IsEdited).AddTo(disposables);

            IsEdited = editableProps.Select(a => a.ObserveProperty(b => b.IsEdited)).CombineLatestValuesAreAllFalse().Inverse().ToReactiveProperty().AddTo(disposables);
            IsEdited.Where(a => !a).Subscribe(_ => editableProps.ToList().ForEach(a => a.IsEdited = false));

            // サブタスクの場合のみ
            if (isSub && issue != null)
            {
                Task.Run(() =>
                {
                    timeEntries = redmine.GetTimeEntries(issue.Id);
                    RaisePropertyChanged(nameof(MySpentHours));
                    RaisePropertyChanged(nameof(DiffEstimatedSpent));
                });
                // アサインが変更された場合は、合計時間の変更通知を行う。
                this.ObserveProperty(a => a.AssignedTo.Value).Subscribe(_ => RaisePropertyChanged(nameof(MySpentHours))).AddTo(disposables);
                this.ObserveProperty(a => a.EstimatedHours.Value).Subscribe(_ => RaisePropertyChanged(nameof(DiffEstimatedSpent))).AddTo(disposables);
            }
        }

        public virtual void Read()
        {
            try
            {
                if (Issue == null) return;
                var issue = redmine.GetIssue(Issue.Id);
                SetIssue(issue);
            }
            catch (System.Net.WebException webEx)
            {
                var ex = webEx.InnerException;
                throw new ApplicationException($"#{this.Id}\n{ex.Message}", ex);
            }
        }

        public virtual void Write()
        {
            try
            {
                if (IsEdited.Value)
                    redmine.UpdateTicket(Issue);
                IsEdited.Value = false;
            }
            catch(System.Net.WebException webEx)
            {
                var ex = webEx.InnerException;
                throw new ApplicationException($"#{this.Id}\n{ex.Message}", ex);
            }
        }
    }
}
