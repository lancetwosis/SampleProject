using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Filters;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public class SpecifyUsersViewModel : FilterGroupViewModelBase
    {
        public ExpandableTwinListBoxViewModel<MyUser> Users { get; set; }

        // https://www.colordic.org/colorsample/fff7f0
        public SpecifyUsersViewModel(TicketFiltersModel model)
            : base(model.ToReactivePropertySlimAsSynchronized(a => a.SpecifyUsers), ColorEx.ToBrush("#fff7f0"))
        {
            var allUsers = CacheManager.Default.Users.Value;
            var selectedUsers = model.Users;

            Users = new ExpandableTwinListBoxViewModel<MyUser>(allUsers, selectedUsers).AddTo(disposables);
            IsEnabled.Skip(1).Subscribe(i =>
            {
                Users.Expanded = i;
                if (i && model.Users.Count == 0)
                {
                    selectedUsers.Add(allUsers.First(a => a.Id == CacheManager.Default.MyUser.Value.Id));
                }
            }).AddTo(disposables);

            var usersChanged = model.Users.CollectionChangedAsObservable().StartWithDefault();
            IsValid = usersChanged.Select(_ => model.Users.Any() ? null : Resources.VisualizeUserErrMsg)
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Label = IsEnabled.CombineLatest(IsValid, usersChanged, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return $"{Resources.VisualizeUser}: {Resources.VisualizeNotSpecified}";
                else if (IsValid.Value != null)
                    return $"{Resources.VisualizeUser} {NAN}";

                if (model.Users.Count <= 3)
                    return $"{string.Join(", ", model.Users)}";
                else
                    return $"{string.Join(", ", model.Users.Take(3))}, ...";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Tooltip = IsEnabled.CombineLatest(IsValid, usersChanged, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return Resources.VisualizeUserMsg;
                else if (IsValid.Value != null)
                    return null;
                else
                    return $"{string.Join(Environment.NewLine, model.Users)}";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
