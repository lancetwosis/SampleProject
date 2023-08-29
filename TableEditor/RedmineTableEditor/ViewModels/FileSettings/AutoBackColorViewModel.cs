using Reactive.Bindings.Extensions;
using RedmineTableEditor.Models.FileSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class AutoBackColorViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public StatusColorsViewModel StatusColors { get; }
        public AssignedToColorsViewModel AssignedToColors { get; }
        public ReactivePropertySlim<bool> NotUse { get; set; }

        public ReactiveProperty<bool> IsEdited { get; set; }

        public AutoBackColorViewModel(AutoBackColorModel model, RedmineManager redmine)
        {
            StatusColors = new StatusColorsViewModel(model.StatusColors, redmine).AddTo(disposables);
            AssignedToColors = new AssignedToColorsViewModel(model.AssignedToColors, redmine).AddTo(disposables);

            NotUse = new ReactivePropertySlim<bool>().AddTo(disposables);
            StatusColors.IsEnabled.CombineLatest(AssignedToColors.IsEnabled, (s, a) => (s, a))
                .Subscribe(p => { NotUse.Value = !p.s && !p.a; }).AddTo(disposables);

            IsEdited = new[]
            {
                // IsEdited が後で更新されるため
                StatusColors.ObserveProperty(a => a.IsEdited.Value).Where(a => a),
                AssignedToColors.IsEdited.Where(a => a),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);

            IsEdited.Where(a => !a).Subscribe(_ =>
            {
                // IsEdited が後で更新されるため
                if(StatusColors.IsEdited != null)
                    StatusColors.IsEdited.Value = false;
                AssignedToColors.IsEdited.Value = false;
            }).AddTo(disposables);
        }
    }
}
