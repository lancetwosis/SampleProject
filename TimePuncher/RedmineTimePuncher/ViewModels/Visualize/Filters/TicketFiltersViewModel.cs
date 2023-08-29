﻿using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
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
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public class TicketFiltersViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public SpecifyParentIssueViewModel SpecifyParentIssue { get; set; }
        public SpecifyPeriodViewModel SpecifyPeriod { get; set; }
        public SpecifyUsersViewModel SpecifyUsers { get; set; }
        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }
        public ReactivePropertySlim<bool> IsExpanded { get; set; }
        public TicketFiltersModel Model { get; set; }

        private VisualizeViewModel parent { get; set; }

        public TicketFiltersViewModel(VisualizeViewModel parent)
        {
            this.parent = parent;

            var json = Properties.Settings.Default.VisualizeFilters;
            if (!string.IsNullOrEmpty(json))
                Model = CloneExtentions.ToObject<TicketFiltersModel>(json);
            else
                Model = new TicketFiltersModel();

            setup();
        }

        private void setup(DateTime? createAt = null)
        {
            IsExpanded = Model.ToReactivePropertySlimAsSynchronized(a => a.IsExpanded).AddTo(disposables);

            SpecifyParentIssue = new SpecifyParentIssueViewModel(Model, parent.Parent.Redmine).AddTo(disposables);
            SpecifyPeriod = new SpecifyPeriodViewModel(Model, createAt).AddTo(disposables);
            SpecifyUsers = new SpecifyUsersViewModel(Model, parent.Parent.Redmine).AddTo(disposables);

            IsValid = SpecifyParentIssue.IsEnabled.CombineLatest(SpecifyParentIssue.IsValid,
                SpecifyPeriod.IsEnabled, SpecifyPeriod.IsValid,
                SpecifyPeriod.IsEnabled, SpecifyPeriod.IsValid, (_1, _2, _3, _4, _5, _6) => true).Select(_ =>
            {
                if (!SpecifyParentIssue.IsEnabled.Value && !SpecifyPeriod.IsEnabled.Value)
                    return "検索条件を選択してください。";

                if (SpecifyParentIssue.IsEnabled.Value && SpecifyParentIssue.IsValid.Value != null)
                    return SpecifyParentIssue.IsValid.Value;
                if (SpecifyPeriod.IsEnabled.Value && SpecifyPeriod.IsValid.Value != null)
                    return SpecifyPeriod.IsValid.Value;
                if (SpecifyUsers.IsEnabled.Value && SpecifyUsers.IsValid.Value != null)
                    return SpecifyUsers.IsValid.Value;

                return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public List<TimeEntry> GetTimeEntries()
        {
            return Model.GetTimeEntries(parent.Parent.Settings.Schedule, parent.Parent.Redmine.Value);
        }

        public async Task<List<TicketModel>> GetTicketsAsync(List<TimeEntry> times)
        {
            return await Model.GetTicketsAsync(times, parent.Parent.Redmine.Value);
        }

        public TicketFiltersViewModel(VisualizeViewModel parent, TicketFiltersModel model, DateTime createAt)
        {
            this.parent = parent;
            this.Model = model;
            setup(createAt);
        }

        public TicketFiltersViewModel Clone(DateTime createAt)
        {
            return new TicketFiltersViewModel(parent, Model.Clone(), createAt);
        }

        public void Save()
        {
            Properties.Settings.Default.VisualizeFilters = Model.ToJson();
            Properties.Settings.Default.Save();
        }
    }
}
