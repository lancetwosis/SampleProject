using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.ViewModels.Bases;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.Visualize.Charts
{
    public abstract class ChartViewModelBase : LibRedminePower.ViewModels.Bases.ViewModelBase
    {

        public ReadOnlyReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsEdited { get; set; }

        public ChartViewModelBase(ViewType type, ResultViewModel parent)
        {
            IsEnabled = parent.ViewType.Select(t => t == type).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        protected CompositeDisposable myDisposables;
        public virtual void SetupSeries()
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);
        }

        protected void setupIsEdited(params FactorTypeViewModel[] factors)
        {
            IsEdited = factors.Select(f => f.IsEdited).CombineLatest().Select(l => l.Any(a => a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
