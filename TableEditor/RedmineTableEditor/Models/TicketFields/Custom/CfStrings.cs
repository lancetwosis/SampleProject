using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Custom
{
    public class CfStrings : Bases.CfBase<ObservableCollection<string>>
    {
        public CfStrings(CustomField meta, IssueCustomField cf)
            : base(meta,
                  cf,
                  () => new ObservableCollection<string>(),
                  (v) => cf.Values = v.Select(b => new CustomFieldValue() { Info = b.ToString() }).ToArray())
        {
            ObservableCollection<string> col;
            if (cf.Values != null && cf.Values.Any())
            {
                col = new ObservableCollection<string>(cf.Values.Select(b => b.Info).ToList());
            }
            else
            {
                col = new ObservableCollection<string>();
            }
            col.CollectionChanged += (s, e) =>
            {
                this.IsEdited = true;
                cf.Values = this.Value.Select(b => new CustomFieldValue() { Info = b }).ToArray();
            };

            this.GetFunc = () => col;
        }
    }
}
