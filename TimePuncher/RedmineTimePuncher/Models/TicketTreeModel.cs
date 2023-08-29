using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class TicketTreeModel : LibRedminePower.Models.Bases.ModelBase
    {
        public List<TicketTreeItemModel> Items { get; set; } = new List<TicketTreeItemModel>();

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> HasItem { get; set; }

        public TicketTreeModel()
        {
            HasItem = this.ObserveProperty(a => a.Items).Select(a => a.Any()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override string ToString()
        {
            return string.Join("\\", Items.Select(a => a.ToString()));
        }
    }
}
