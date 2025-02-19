using LibRedminePower.Extentions;
using LibRedminePower.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class EstimatedHours : Bases.FieldDouble, IDisposable
    {
        public static double MAX { get; set; }

        public double Max => MAX;

        private CompositeDisposable disposables = new CompositeDisposable();

        public EstimatedHours(Issue issue) :base(
            Resources.enumIssuePropertyTypeEstimatedHours,
            () =>
            {
                if (issue == null) return null;
                if (issue.EstimatedHours == null) return null;

                var myDeci = (decimal)issue?.EstimatedHours;
                var result = decimal.ToDouble(myDeci);
                return result;
            },
            (v) =>
            {
                if (v.HasValue)
                    issue.EstimatedHours = (float)v;
            })
        {
            this.ObserveProperty(a => a.Value).Where(a => a.HasValue).SubscribeWithErr(v =>
            {
                if (v.Value > MAX)
                    MAX = v.Value;
            }).AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
