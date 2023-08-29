using Redmine.Net.Api.Types;
using RedmineTableEditor.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Custom
{
    public class CfInts : Bases.CfBase<ObservableCollection<int>>
    {
        public CfInts(CustomField meta, IssueCustomField cf)
            : base(meta,
                  cf,
                  () => new ObservableCollection<int>(),
                  (v) => cf.Values = v.Select(b => new CustomFieldValue() { Info = b.ToString() }).ToArray())
        {
            ObservableCollection<int> col;
            if (cf.Values != null && cf.Values.Any())
            {
                col = new ObservableCollection<int>(cf.Values.Select(b => int.Parse(b.Info)).ToList());
            }
            else
            {
                col = new ObservableCollection<int>();
            }
            col.CollectionChanged += (s, e) =>
            {
                this.IsEdited = true;
                cf.Values = this.Value.Select(b => new CustomFieldValue() { Info = b.ToString() }).ToArray();
            };

            this.GetFunc = () => col;
        }
    }
}
