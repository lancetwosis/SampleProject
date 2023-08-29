using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Input.Bases
{
    public abstract class ResourceViewModelBase<T> : LibRedminePower.ViewModels.Bases.ViewModelBase where T : MyResourceBase
    {
        public T Resource { get; set; }

        public BusyNotifier NowUpdating { get; set; }

        public ResourceViewModelBase()
        {
            NowUpdating = new BusyNotifier();
        }

        /// <summary>
        /// 処理の前後で予定の選択状態を保ったまま処理を実施する
        /// </summary>
        protected async Task execUpdateAsync(InputViewModel parent, Func<Task> updater)
        {
            var preSelected = parent.SelectedAppointments.Value;

            using (NowUpdating.ProcessStart())
            {
                await updater.Invoke();
            }

            if (preSelected != null && preSelected.Any())
            {
                parent.SelectedAppointments.Value = preSelected.Select(a => parent.Appointments.FirstOrDefault(b => b.IsSame(a)))
                                                               .Where(a => a != null)
                                                               .ToList();
            }
        }

        /// <summary>
        /// 処理の前後で予定の選択状態を保ったまま処理を実施する
        /// </summary>
        protected void execUpdate(InputViewModel parent, Action updater)
        {
            var preSelected = parent.SelectedAppointments.Value;

            using (NowUpdating.ProcessStart())
            {
                updater.Invoke();
            }
 
            if (preSelected != null && preSelected.Any())
            {
                parent.SelectedAppointments.Value = preSelected.Select(a => parent.Appointments.FirstOrDefault(b => b.IsSame(a)))
                                                               .Where(a => a != null)
                                                               .ToList();
            }
        }
    }
}
