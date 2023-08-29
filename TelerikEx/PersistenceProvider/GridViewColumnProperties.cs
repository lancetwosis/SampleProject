using LibRedminePower.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using TelerikEx;

namespace TelerikEx.PersistenceProvider
{
    public class GridViewColumnProperties
    {
        public string UniqueName;
        public List<ColumnProxy> Items { get; set; } = new List<ColumnProxy>();
        
        public GridViewColumnProperties(string uniqueName)
        {
            UniqueName = uniqueName;
        }

        public void Apply(RadGridView gridView)
        {
            var columns = gridView.Columns.Cast<GridViewColumn>().ToList();

            if (!Items.Any(c => c.Width.IsStar))
            {
                foreach (var columnProp in Items)
                {
                    var column = columns.SingleOrDefault(i => i.UniqueName == columnProp.UniqueName);
                    if (column != null)
                    {
                        column.DisplayIndex = columnProp.DisplayIndex;
                        column.Width = columnProp.Width;
                    }
                }

                var sortedProp = Items.FirstOrDefault(p => p.SortingState != SortingState.None);
                var sortedCol = columns.FirstOrDefault(c => c.UniqueName == sortedProp?.UniqueName);
                if (sortedCol != null)
                {
                    var csd = new ColumnSortDescriptor()
                    {
                        Column = sortedCol,
                        SortDirection = sortedProp.SortingState == SortingState.Ascending ?
                            ListSortDirection.Ascending : ListSortDirection.Descending,
                    };
                    gridView.SortDescriptors.Add(csd);
                }
            }
            // #49 保存していたカラム情報に width="*" が含まれていた場合への対応
            // チケット一覧の Subject に "*" が使われていたため、この処理を実施する
            // 本対応と同時に "*" のカラムは削除したので、本処理が適用されるのはバージョンアップ前後のみ
            else
            {
                // "*" でないカラムだけ先に追加する
                var notStarColProps = Items.Where(c => !c.Width.IsStar).ToList();
                foreach (var columnProp in notStarColProps)
                {
                    var column = columns.SingleOrDefault(i => i.UniqueName == columnProp.UniqueName);
                    if (column != null)
                    {
                        column.DisplayIndex = columnProp.DisplayIndex;
                        column.Width = columnProp.Width;
                    }
                }

                // "*" が指定されていたカラムが一つの場合は 250 に設定する
                var starColProps = Items.Where(c => c.Width.IsStar).ToList();
                var starColWidth = 250 / starColProps.Count;
                foreach (var columnProp in starColProps)
                {
                    var column = columns.SingleOrDefault(i => i.UniqueName == columnProp.UniqueName);
                    if (column != null)
                    {
                        column.DisplayIndex = columnProp.DisplayIndex;
                        column.Width = new GridViewLength(starColWidth);
                    }
                }

                var sortedProp = Items.FirstOrDefault(p => p.SortingState != SortingState.None);
                var sortedCol = columns.FirstOrDefault(c => c.UniqueName == sortedProp?.UniqueName);
                if (sortedCol != null)
                {
                    var csd = new ColumnSortDescriptor()
                    {
                        Column = sortedCol,
                        SortDirection = sortedProp.SortingState == SortingState.Ascending ?
                            ListSortDirection.Ascending : ListSortDirection.Descending,
                    };
                    gridView.SortDescriptors.Add(csd);
                }
            }
        }

        public void Restore(RadGridView gridView)
        {
            Items.Clear();
            Items.AddRange(gridView.Columns.Cast<GridViewColumn>().ToList().Select(c => new ColumnProxy(c)));
        }
    }
}
