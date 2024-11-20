using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common
{
    public interface IRequestPeriod
    {
        ReactivePropertySlim<DateTime> StartDate { get; set; }
        ReactivePropertySlim<DateTime> DueDate { get; set; }
        ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        void Clear();
    }
}
