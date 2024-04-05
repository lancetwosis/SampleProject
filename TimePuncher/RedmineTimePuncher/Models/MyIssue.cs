using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyIssue : IdName, IComparable<MyIssue>
    {
        public static string UrlBase;
        public static Task<List<IssuePriority>> PrioritiesTask;
        public static Task<List<IssueStatus>> StatussTask { get; set; }
        public static Task<List<Tracker>> TrakersTask { get; set; }

        public int? ParentId { get; set; }
        public MyPriority Priority { get; set; } = new MyPriority();
        public IdName Project { get; set; } = new IdName();
        public MyTracker Tracker { get; set; } = new MyTracker();
        public string Subject { get; set; }
        public IdName Status { get; set; } = new IdName();
        public IdName Category { get; set; } = new IdName();
        public IdName AssignedTo { get; set; } = new IdName();
        public IdName FixedVersion { get; set; } = new IdName();
        public DateTime? DueDate { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public bool IsClosed { get; set; }
        public bool IsHgihPriority { get; set; }
        public bool IsExpired { get; set; }

        public bool IsFavorite { get; set; }

        public string Url => GetUrl(Id);
        public string Label => $"{Tracker.Name} #{Id} {Subject}";
        public string LimitedDescription => string.IsNullOrEmpty(RawIssue.Description) ? null : RawIssue.Description.LimitRows(20);

        public Issue RawIssue { get; set; }

        [Obsolete("For Serialize", true)]
        public MyIssue()
        {
            initRx(2);
        }

        public MyIssue(Issue issue)
        {
            Id = issue.Id;
            Name = issue.Subject;

            RawIssue = issue;

            Id = issue.Id;
            ParentId = issue.ParentIssue?.Id;
            Priority = new MyPriority(issue.Priority);
            Project = new IdName(issue.Project);
            Tracker = new MyTracker(issue.Tracker);
            Subject = issue.Subject;
            Status = new IdName(issue.Status);
            Category = new IdName(issue.Category);
            AssignedTo = new IdName(issue.AssignedTo);
            FixedVersion = new IdName(issue.FixedVersion);

            DueDate = issue.DueDate;
            UpdatedOn = issue.UpdatedOn;

            IsExpired = DueDate < DateTime.Today;
            IsClosed = StatussTask.Result.First(s => s.Id == issue.Status.Id).IsClosed;

            var priority = PrioritiesTask.Result.Indexed().First(a => a.v.Id == Priority.Id);
            var defaultPriority = PrioritiesTask.Result.Indexed().First(a => a.v.IsDefault);
            IsHgihPriority = PrioritiesTask.Result.Indexed().FirstOrDefault(a => a.v.Id == Priority.Id).i > defaultPriority.i;

            initRx(1);
        }

        private void initRx(int skipCount)
        {
            this.ObserveProperty(a => a.Project.Id).Skip(skipCount).SubscribeWithErr(id => throw new InvalidProgramException("Not supported Project.Id")).AddTo(disposables);
            this.ObserveProperty(a => a.Tracker.Id).Skip(skipCount).SubscribeWithErr(id => Tracker.Name = TrakersTask.Result.FirstOrDefault(a => a.Id == id)?.Name).AddTo(disposables);
            this.ObserveProperty(a => a.Status.Id).Skip(skipCount).SubscribeWithErr(id =>
            {
                var status = StatussTask.Result.FirstOrDefault(a => a.Id == id);
                if(status != null)
                {
                    Status.Name = status.Name;
                    IsClosed = status.IsClosed;
                }
                
            }).AddTo(disposables);
            this.ObserveProperty(a => a.Category.Id).Skip(skipCount).SubscribeWithErr(id => throw new InvalidProgramException("Not supported Category.Id")).AddTo(disposables);
            this.ObserveProperty(a => a.AssignedTo.Id).Skip(skipCount).SubscribeWithErr(id => AssignedTo.Name = "").AddTo(disposables);
            this.ObserveProperty(a => a.FixedVersion.Id).Skip(skipCount).SubscribeWithErr(id => FixedVersion.Name = "").AddTo(disposables);
        }

        public void GoToTicket()
        {
            System.Diagnostics.Process.Start(Url);
        }

        /// <summary>
        /// 本チケットを親チケットとし、「プロジェクト」「対象バージョン」「カテゴリー」「優先度」「開始日」「期日」を引き継いだチケットを返す。
        /// </summary>
        public Issue CreateChildTicket()
        {
            var child = new Issue();
            child.ParentIssue = this.ToIdentifiableName();
            child.Project = RawIssue.Project;
            child.FixedVersion = RawIssue.FixedVersion;
            child.Category = RawIssue.Category;
            child.Priority = RawIssue.Priority;
            child.StartDate = RawIssue.StartDate;
            child.DueDate = RawIssue.DueDate;
            return child;
        }

        public override string ToString()
        {
            return $"{Tracker.Name.Replace(",", " ")} #{Id} ({Status.Name.Replace(",", " ")}): {Subject.Replace(",", " ")}";
        }

        public string ToString(Journal journal)
        {
            if(journal == null || journal.Details == null ) return ToString();
            var statusDetail = journal.Details.FirstOrDefault(a => a.Name == "status_id");
            if (statusDetail == null) return ToString();
            var statusOld = StatussTask.Result.FirstOrDefault(a => a.Id.ToString() == statusDetail.OldValue)?.Name;
            if(string.IsNullOrEmpty(statusOld)) return ToString();
            var statusNew = StatussTask.Result.FirstOrDefault(a => a.Id.ToString() == statusDetail.NewValue)?.Name;
            if (string.IsNullOrEmpty(statusNew)) return ToString();
            if (statusOld == statusNew) return ToString();

            return $"{Tracker.Name} #{Id} ({statusOld} → {statusNew}): {Subject}";
        }

        public override bool Equals(object obj)
        {
            var myIssue = obj as MyIssue;
            if (myIssue == null) return false;
            return myIssue.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }

        public int CompareTo(MyIssue other)
        {
            var prioDiff = other.Priority.Id - Priority.Id;
            if (prioDiff != 0) return prioDiff;

            if (DueDate.HasValue && other.DueDate.HasValue)
                return Convert.ToInt32((DueDate.Value - other.DueDate.Value).TotalMinutes);
            else if (DueDate.HasValue && !other.DueDate.HasValue)
                return -1;
            else if (!DueDate.HasValue && other.DueDate.HasValue)
                return 1;

            return 0;
        }

        public static string GetUrl(int id)
        {
            return $"{UrlBase}issues/{id}";
        }
    }
}
