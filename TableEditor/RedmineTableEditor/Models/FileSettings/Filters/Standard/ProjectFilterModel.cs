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
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using LibRedminePower.Properties;

namespace RedmineTableEditor.Models.FileSettings.Filters.Standard
{
    public class ProjectFilterModel : ItemsFilterModelBase
    {
        public static FilterItemModel MINE = new FilterItemModel("mine", Properties.Resources.FilterProjectMine);

        public ProjectFilterModel() : base(Resources.enumIssuePropertyTypeProject, RedmineKeys.PROJECT_ID, true)
        {
            // プロジェクトに関しては使用頻度と処理の手間を考えて「等しくない」の選択肢は外す
            CompareTypes = new List<CompareTypeModel>() { CompareTypeModel.EQUALS };
        }

        /// <summary>
        /// 選択された単一のプロジェクトのIDを返す。未選択だったり、複数選択されていたら例外発報。
        /// </summary>
        public string GetSelectedProjectId()
        {
            if (IsMultiple && Items.Count == 1)
            {
                return Items[0].Id;
            }
            else if (!IsMultiple && SelectedItem != null)
            {
                return SelectedItem.Id;
            }
            else
            {
                // 現状、カテゴリによるフィルターのみなのでこのメッセージとする
                throw new ApplicationException(Properties.Resources.FilterErrMsgSingleProjectForCategory);
            }
        }
    }
}
