using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings.Filters;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings.Filters.Bases
{
    public class TextIssueFilterViewModelBase<T> : TextFilterViewModelBase<T>
        where T : TextIssueFilterModelBase, new()
    {
        public ReadOnlyReactivePropertySlim<bool> ShowGoToTicket { get; set; }
        public ReactiveCommand GoToTicketCommand { get; set; }

        public TextIssueFilterViewModelBase(T model) : base(model)
        {
            ShowGoToTicket = Text.Select(a => a != null && Regex.IsMatch(a, "^[0-9]+$")).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            GoToTicketCommand = ShowGoToTicket.ToReactiveCommand().WithSubscribe(() =>
            {
                Process.Start(redmine.GetIssueUrl(int.Parse(Text.Value)));
            }).AddTo(myDisposables);
        }
    }
}
