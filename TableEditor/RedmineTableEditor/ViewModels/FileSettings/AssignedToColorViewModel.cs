using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.TicketFields.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class AssignedToColorViewModel : LibRedminePower.ViewModels.Bases.ColorSettingViewModelBase
    {
        public ReactivePropertySlim<IdentifiableName> User { get; set; }
        public ReactivePropertySlim<int> Order { get; set; }

        public ReactiveProperty<bool> IsEdited { get; set; }
        public AssignedToColorModel Model { get; set; }

        public AssignedToColorViewModel()
        {
            setModel(new AssignedToColorModel());
        }

        public AssignedToColorViewModel(AssignedToColorModel model)
        {
            setModel(model);
        }

        private void setModel(AssignedToColorModel model)
        {
            this.Model = model;
            User = model.ToReactivePropertySlimAsSynchronized(a => a.Name, a => AssignedToColorsViewModel.Redmine.Users?.FirstOrDefault(u => u.Name == a), a => a?.Name);
            Order = model.ToReactivePropertySlimAsSynchronized(a => a.Order).AddTo(disposables);
            Color = model.ToReactivePropertySlimAsSynchronized(a => a.Color, a => a.ToMediaColor(), a => a.ToDrawingColor()).AddTo(disposables);

            User.Value = AssignedToColorsViewModel.Redmine.Users?.FirstOrDefault();

            IsEdited = new[]
            {
                User.Skip(1).Select(_ => true),
                Order.Skip(1).Select(_ => true),
                Color.Skip(1).Select(_ => true),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);
        }
    }
}
