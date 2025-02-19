using LibRedminePower.Enums;
using LibRedminePower.Extentions;
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
using RedmineTableEditor.Models.MyTicketFields;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.Models.TicketFields.Custom;
using RedmineTableEditor.Models.TicketFields.Custom.Bases;
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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.Bases
{
    public abstract class MyIssueBase : LibRedminePower.Models.Bases.ModelBase
    {
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
        public SpentHours SpentHours { get; set; }
        public float? TotalSpentHours { get; set; }
        public float? TotalEstimatedHours { get; set; }

        public string Project { get; set; }
        public string Author { get; set; }
        public string LastUpdater { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }

        public MySpentHours MySpentHours { get; set; }
        public DiffEstimatedSpent DiffEstimatedSpent { get; set; }
        public ReplyCount ReplyCount { get; set; }
        public RequiredDays RequiredDays { get; set; }
        public DaysUntilCreated DaysUntilCreated { get; set; }

        // 当初は、カスタムフィールドのプロパティは、型毎に作らずに、一つのプロパティにまとめるため、Generic型のプロパティを定義していた。
        // Generic型のプロパティの場合、GridView側が型判断が出来ずに、コピペができないことがわかった。
        // よって、多少煩雑であるが型毎にプロパティを用意した。
        public CfDictionary<CfString, string> DicCustomFieldString { get; set; }
        public CfDictionary<CfFloat, double?> DicCustomFieldFloat { get; set; }
        public CfDictionary<CfBool, bool?> DicCustomFieldBool { get; set; }
        public CfDictionary<CfInt, int?> DicCustomFieldInt { get; set; }
        public CfDictionary<CfDate, DateTime?> DicCustomFieldDate { get; set; }
        public CfDictionary<CfInts, ObservableCollection<int>> DicCustomFieldInts { get; set; }
        public CfDictionary<CfStrings, ObservableCollection<string>> DicCustomFieldStrings { get; set; }

        protected RedmineManager redmine { get; }
        private ObservableCollection<FieldModel> properties { get; set; }

        public MyIssueBase(Issue issue, RedmineManager redmine, ObservableCollection<FieldModel> properties)
        {
            this.redmine = redmine;
            this.properties = properties;
            SetIssue(issue);
        }

        private CompositeDisposable myDisposables { get; set; }
        public void SetIssue(Issue issue)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            this.Issue = issue;
            this.Url = issue != null ? redmine.GetIssueUrl(issue.Id) : "";
            this.Project = issue != null ? issue.Project?.Name : "";
            this.Author = issue != null ? issue.Author?.Name : "";
            this.Created = issue != null ? issue.CreatedOn : null;
            this.Updated = issue != null ? issue.UpdatedOn : null;

            DicCustomFieldString = new CfDictionary<CfString, string>(CfString.DUMMY);
            DicCustomFieldFloat = new CfDictionary<CfFloat, double?>(CfFloat.DUMMY);
            DicCustomFieldBool = new CfDictionary<CfBool, bool?>(CfBool.DUMMY);
            DicCustomFieldInt = new CfDictionary<CfInt, int?>(CfInt.DUMMY);
            DicCustomFieldDate = new CfDictionary<CfDate, DateTime?>(CfDate.DUMMY);
            DicCustomFieldInts = new CfDictionary<CfInts, ObservableCollection<int>>(CfInts.DUMMY);
            DicCustomFieldStrings = new CfDictionary<CfStrings, ObservableCollection<string>>(CfStrings.DUMMY);

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
            editableProps.Add(EstimatedHours = new EstimatedHours(issue).AddTo(myDisposables));

            if (issue != null && issue.CustomFields != null && issue.CustomFields.Any())
            {
                var fields =issue.CustomFields.Select(a => a.ToFieldBase(redmine.Cache.CustomFields.Single(b => b.Id == a.Id))).Where(a => a != null).ToList();
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

            editableProps.ObserveElementProperty(a => a.IsEdited).SubscribeWithErr(a =>
            {
                MyIssueEditedListener.EditedChanged?.Invoke(this, a.Value);
            }).AddTo(myDisposables);

            var editedProps = editableProps.ToFilteredReadOnlyObservableCollection(a => a.IsEdited).AddTo(myDisposables);

            IsEdited = editableProps.Select(a => a.ObserveProperty(b => b.IsEdited)).CombineLatestValuesAreAllFalse().Inverse().ToReactiveProperty().AddTo(myDisposables);
            IsEdited.Where(a => !a).SubscribeWithErr(_ => editableProps.ToList().ForEach(a => a.IsEdited = false));

            RequiredDays = new RequiredDays(Issue, redmine).AddTo(myDisposables);

            if (Issue == null)
                return;

            // 追加の情報が必要な場合、非同期で取得を行う
            if (properties.Any(p => p.IsType(MyIssuePropertyType.MySpentHours) ||
                                    p.IsType(MyIssuePropertyType.DiffEstimatedSpent)))
            {
                var _ = Task.Run(() =>
                {
                    MySpentHours = new MySpentHours(Issue, redmine, AssignedTo).AddTo(myDisposables);
                    DiffEstimatedSpent = new DiffEstimatedSpent(Issue, EstimatedHours, MySpentHours, Status).AddTo(myDisposables);
                });
            }

            var needsJournal = properties.Any(p => p.IsType(MyIssuePropertyType.ReplyCount) ||
                                                   p.IsType(IssuePropertyType.LastUpdater));
            var needsDetail = properties.Any(p => p.IsType(IssuePropertyType.SpentHours) ||
                                                  p.IsType(IssuePropertyType.TotalSpentHours) ||
                                                  p.IsType(IssuePropertyType.TotalEstimatedHours));
            if (needsJournal)
            {
                var _ = Task.Run(() =>
                {
                    var i = redmine.GetIssueIncludeJournals(Issue.Id);
                    ReplyCount = new ReplyCount(i, redmine).AddTo(myDisposables);
                    LastUpdater = i.Journals?.LastOrDefault(j => j.User != null)?.User.Name;
                    SpentHours = new SpentHours(i).AddTo(myDisposables);
                    TotalSpentHours = i.TotalSpentHours;
                    TotalEstimatedHours = i.TotalEstimatedHours;
                });
            }
            else if (needsDetail)
            {
                var _ = Task.Run(() =>
                {
                    var i = redmine.GetIssueFromCache(Issue.Id, true);
                    SpentHours = new SpentHours(i).AddTo(myDisposables);
                    TotalSpentHours = i.TotalSpentHours;
                    TotalEstimatedHours = i.TotalEstimatedHours;
                });
            }

            if (properties.Any(p => p.IsType(MyIssuePropertyType.DaysUntilCreated)))
            {
                var _ = Task.Run(() =>
                {
                    DaysUntilCreated = new DaysUntilCreated(Issue, redmine).AddTo(myDisposables);
                });
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

        public bool IsEnabledCustomField(CustomField cf)
        {
            var format = cf.ToFieldFormat();
            switch (format)
            {
                case FieldFormat.@string:
                case FieldFormat.text:
                case FieldFormat.link:
                case FieldFormat.list:              return DicCustomFieldString.ContainsKey(cf.Id);
                case FieldFormat.@float:            return DicCustomFieldFloat.ContainsKey(cf.Id);
                case FieldFormat.@bool:             return DicCustomFieldBool.ContainsKey(cf.Id);
                case FieldFormat.@int:
                case FieldFormat.user:
                case FieldFormat.version:
                case FieldFormat.enumeration:       return DicCustomFieldInt.ContainsKey(cf.Id);
                case FieldFormat.date:              return DicCustomFieldDate.ContainsKey(cf.Id);
                case FieldFormat.version_multi:
                case FieldFormat.user_multi:        return DicCustomFieldInts.ContainsKey(cf.Id);
                case FieldFormat.list_multi:
                case FieldFormat.enumeration_multi: return DicCustomFieldStrings.ContainsKey(cf.Id);
                default:
                    throw new NotSupportedException($"fieldFormat が {format} は、サポート対象外です。");
            }
        }
    }

    public class CfDictionary<TCfBase, TValue> : Dictionary<int, TCfBase> where TCfBase : CfBase<TValue>
    {
        // Binding で DicCustomFieldString[20].Value のようにアクセスして
        // 要素がなかった場合でも例外にならないよう以下のように処理を上書きする
        new public TCfBase this[int key]
        {
            get
            {
                if (this.TryGetValue(key, out var v))
                    return v;
                else
                    return dummy;
            }
            set
            {
                base[key] = value;
            }
        }

        private TCfBase dummy { get; }
        public CfDictionary(TCfBase dummy) : base()
        {
            this.dummy = dummy;
        }
    }
}
