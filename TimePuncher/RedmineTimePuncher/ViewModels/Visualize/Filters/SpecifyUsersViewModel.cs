﻿using LibRedminePower.Extentions;
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
        public SpecifyUsersViewModel(TicketFiltersModel model, ReactivePropertySlim<RedmineManager> redmine)
            : base(model.ToReactivePropertySlimAsSynchronized(a => a.SpecifyUsers), Color.FromRgb(0xFF, 0xF7, 0xF0))
        {
            CompositeDisposable myDisposables = null;
            redmine.Where(r => r != null).Subscribe(r =>
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                var allUsers = r.GetUsers();
                var selectedUsers = model.Users;

                Users = new ExpandableTwinListBoxViewModel<MyUser>(allUsers, selectedUsers).AddTo(myDisposables);
                IsEnabled.Skip(1).Subscribe(i =>
                {
                    Users.Expanded = i;
                    if (i && model.Users.Count == 0)
                    {
                        selectedUsers.Add(allUsers.First(a => a.Id == r.MyUser.Id));
                    }
                }).AddTo(myDisposables);
            });

            var usersChanged = model.Users.CollectionChangedAsObservable().StartWithDefault();
            IsValid = usersChanged.Select(_ =>
            {
                if (model.Users.Any())
                    return null;
                else
                    return "ユーザを選択してください。";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Label = IsEnabled.CombineLatest(IsValid, usersChanged, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return $"ユーザ: 指定なし";
                else if (IsValid.Value != null)
                    return $"ユーザ: {NAN}";

                if (model.Users.Count <= 3)
                    return $"{string.Join(", ", model.Users)}";
                else
                    return $"{string.Join(", ", model.Users.Take(3))}, ...";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Tooltip = IsEnabled.CombineLatest(IsValid, usersChanged, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return "あなたにアサインされているプロジェクトのメンバーが対象となります";
                else if (IsValid.Value != null)
                    return null;
                else
                    return $"{string.Join(Environment.NewLine, model.Users)}";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
