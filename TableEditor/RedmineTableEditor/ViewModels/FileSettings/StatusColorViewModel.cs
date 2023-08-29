using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.FileSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class StatusColorViewModel : LibRedminePower.ViewModels.Bases.ColorSettingViewModelBase
    {
        public string Name { get; }
        public ReactivePropertySlim<int> StatusId { get; }
        public StatusColorModel Model { get; internal set; }

        public StatusColorViewModel(IssueStatus status, StatusColorModel model)
        {
            this.Model = model;
            Name = status.Name;
            StatusId = model.ToReactivePropertySlimAsSynchronized(a => a.Id).AddTo(disposables);
            Color = model.ToReactivePropertySlimAsSynchronized(a => a.Color, a => a.ToMediaColor(), a => a.ToDrawingColor()).AddTo(disposables);
        }
    }
}
