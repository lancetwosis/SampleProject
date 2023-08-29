using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public class SpecifyParentIssueViewModel : FilterGroupViewModelBase
    {
        public ReactivePropertySlim<string> ParentIssueId { get; set; }

        // https://www.colordic.org/colorsample/f0fff7
        public SpecifyParentIssueViewModel(TicketFiltersModel model, ReactivePropertySlim<RedmineManager> redmine)
            : base(model.ToReactivePropertySlimAsSynchronized(a => a.SpecifyParentIssue), Color.FromRgb(0xF0, 0xFF, 0xF7))
        {
            ParentIssueId = model.ToReactivePropertySlimAsSynchronized(a => a.ParentIssueId).AddTo(disposables);

            var parentIssue = ParentIssueId.StartWithDefault().CombineLatest(redmine, (no, r) => (no, r)).Where(p => p.no != null && p.r != null)
                .Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnUIDispatcher().Select(p =>
                {
                    var no = p.no.Trim().TrimStart('#');
                    if (Regex.IsMatch(no, "^[0-9]+$"))
                    {
                        try
                        {
                            return redmine.Value.GetTicketsById(no);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsValid = parentIssue.Select(i => i != null ? null : "親チケットを指定してください。").ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Label = IsEnabled.CombineLatest(IsValid, parentIssue, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return $"親チケット: 指定なし";
                else if (IsValid.Value != null)
                    return $"親チケット: {NAN}";
                else
                    return $"親チケット: #{ParentIssueId.Value}";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Tooltip = IsEnabled.CombineLatest(IsValid, parentIssue, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return "アサインされているプロジェクトのチケットが対象となります。";
                else if (IsValid.Value != null)
                    return null;
                else
                    return $"{parentIssue.Value.RawIssue.GetFullLabel()}";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
