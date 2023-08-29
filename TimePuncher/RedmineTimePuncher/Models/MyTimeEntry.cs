using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;

public class MyTimeEntry : LibRedminePower.Models.Bases.ModelBase, IPeriod
{
    public static Dictionary<int, MyUser> DicUsers { get; set; }
    public static Dictionary<int, MyCategory> DicCategory { get; set; }

    private class TimeEntryOption
    {
        public string Subject { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<int> ToMembers { get; set; }
        public List<int> ToMemberCategoryIds { get; set; }
        public int FromId { get; set; }
        public TimeEntryType Type { get; set; }

        public TimeEntryOption() {}

        public TimeEntryOption(DateTime start, DateTime end, List<int> memberIds, List<int> toMemberCategories, int fromId, TimeEntryType type)
        {
            this.Start = start;
            this.End = end;
            this.ToMembers = memberIds;
            this.ToMemberCategoryIds = toMemberCategories;
            this.FromId = fromId;
            this.Type = type;
        }
    }

    public int? ProjectId 
    {
        get { return Entry.Project?.Id; }
        set { Entry.Project = IdentifiableName.Create<IdentifiableName>(value.Value); }
    }
    public int? IssueId
    {
        get { return Entry.Issue?.Id; }
        set { Entry.Issue = IdentifiableName.Create<IdentifiableName>(value.Value); }
    }
    public int? ActivityId
    {
        get { return Entry.Activity?.Id; }
        set { Entry.Activity = IdentifiableName.Create<IdentifiableName>(value.Value); }
    }
    public string ActivityName => DicCategory[ActivityId.Value].DisplayName;
    public MyCategory Activity => DicCategory[ActivityId.Value];
    public int? UserId => Entry.User?.Id;
    public string UserName => DicUsers[UserId.Value].Name;

    public DateTime? SpentOn
    {
        get { return Entry.SpentOn; }
        set { Entry.SpentOn = value; }
    }
    public DateTime Start
    {
        get{ return option.Start; }
        set
        {
            option.Start = value;
            Entry.Comments = option.ToJson(false);
            Entry.Hours = Convert.ToDecimal((double)(End - Start).TotalHours);
        }
    }
    public DateTime End
    {
        get { return option.End; }
        set
        {
            option.End = value;
            Entry.Comments = option.ToJson(false);
            Entry.Hours = Convert.ToDecimal((double)(End - Start).TotalHours);
        }
    }
    public List<int> ToMembers
    {
        get { return option.ToMembers; }
        set
        {
            option.ToMembers = value;
            Entry.Comments = option.ToJson(false);
        }
    }
    public List<int> ToMemberCategoryIds
    {
        get { return option.ToMemberCategoryIds; }
        set
        {
            option.ToMemberCategoryIds = value;
            Entry.Comments = option.ToJson(false);
        }
    }
    public int FromId
    {
        get { return option.FromId; }
        set
        {
            option.FromId = value;
            Entry.Comments = option.ToJson(false);
        }
    }
    public string Subject
    {
        get { return option.Subject; }
        set
        {
            option.Subject = value;
            Entry.Comments = option.ToJson(false);
        }
    }
    public TimeEntryType Type
    {
        get { return option.Type; }
        set
        {
            option.Type = value;
            Entry.Comments = option.ToJson(false);
        }
    }

    public TimeEntry Entry { get; } = new TimeEntry();
    private TimeEntryOption option = new TimeEntryOption();

    public MyTimeEntry(TimeEntry timeEntry)
    {
        Entry = timeEntry;
        option = CloneExtentions.ToObject<TimeEntryOption>(timeEntry.Comments);
        Entry.Hours = Convert.ToDecimal((double)(End - Start).TotalHours);
    }

    public MyTimeEntry(MyAppointment apo)
    {
        var issue = apo.Ticket;
        ProjectId = issue.Project.Id;
        IssueId = issue.Id;
        SpentOn = apo.Start;
        if(apo.Category != null && apo.Category is MyCategory myCategory)
            ActivityId = myCategory.Id;
        Start = apo.Start;
        End = apo.End;
        ToMembers = apo.MemberAppointments.Select(a => int.Parse(a.Resources.First().ResourceName)).ToList();
        ToMemberCategoryIds = apo.MemberAppointments.Select(a => (a.Category as MyCategory).Id).ToList();
        FromId = apo.FromEntryId;
        Subject = apo.Subject;
    }

    /// <summary>
    /// 作業実績の計算用に追加。
    /// </summary>
    public MyTimeEntry(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public MyTimeEntry Clone()
    {
        return new MyTimeEntry(Entry.Clone() as TimeEntry);
    }

    public override string ToString()
    {
        return $"{Type}: {Start} - {End} ({Entry.Hours} h)";
    }
}