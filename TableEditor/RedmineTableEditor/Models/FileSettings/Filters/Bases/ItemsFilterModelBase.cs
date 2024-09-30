using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Enums;
using Telerik.Windows.Controls;
using System.Windows.Data;
using LibRedminePower.Extentions;
using System.Collections.Concurrent;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System.Collections.ObjectModel;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Models.Bases;
using System.Threading;
using RedmineTableEditor.Enums;
using System.Text.Json.Serialization;
using LibRedminePower.Models;

namespace RedmineTableEditor.Models.FileSettings.Filters.Bases
{
    public class ItemsFilterModelBase : FilterModelBase
    {
        public bool IsMultiple { get; set; }
        public FilterItemModel SelectedItem { get; set; }
        public ObservableCollection<FilterItemModel> Items { get; set; }
        public List<FilterItemModel> AllItems { get; set; }

        public ItemsFilterModelBase()
        {
        }

        public ItemsFilterModelBase(string name, string key, bool isRequired) : base(name, key)
        {
            Items = new ObservableCollection<FilterItemModel>();
            CompareTypes = new List<CompareTypeModel>()
            {
                CompareTypeModel.EQUALS,
                CompareTypeModel.NOT_EQUALS,
            };
            if (!isRequired)
            {
                CompareTypes.Add(CompareTypeModel.NONE);
                CompareTypes.Add(CompareTypeModel.ANY);
            }
        }

        public override string GetQuery()
        {
            if (CompareType.Equals(CompareTypeModel.EQUALS) || CompareType.Equals(CompareTypeModel.NOT_EQUALS))
                return IsMultiple ? createQuery(Items.ToArray()) : createQuery(SelectedItem);
            else
                return createQuery();
        }

        private string createQuery(params FilterItemModel[] items)
        {
            // 以下のような文字列を作る
            // &f[]=tracker_id&op[tracker_id]=!&v[tracker_id][]=2&v[tracker_id][]=4
            var query = new StringBuilder();
            query.Append($"&f[]={RedmineKey}&op[{RedmineKey}]");
            query.Append($"{CompareType.GetEqualSign()}");
            foreach (var i in items.Where(a => a != null))
            {
                query.Append($"&v[{RedmineKey}][]={i.Id}");
            }
            return query.ToString();
        }

        public override string Validate()
        {
            if (!NeedsFilter || !CompareType.NeedsInput())
                return null;

            if (CompareType.Equals(CompareTypeModel.EQUALS) || CompareType.Equals(CompareTypeModel.NOT_EQUALS))
            {
                if (IsMultiple)
                {
                    if (Items.Count == 0)
                        return string.Format(Properties.Resources.FilterErrMsgSelectValue, Name);
                }
                else
                {
                    if (SelectedItem == null)
                        return string.Format(Properties.Resources.FilterErrMsgSelectValue, Name);
                }
            }

            return null;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            IsMultiple = false;
            SelectedItem = AllItems?.Count > 0 ? AllItems.First() : null;
            Items.Clear();
        }
    }
}
