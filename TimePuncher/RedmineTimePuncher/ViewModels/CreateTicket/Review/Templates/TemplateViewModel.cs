using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using ObservableCollectionSync;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using RedmineTimePuncher.Views.CreateTicket.Review;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates
{
    public class TemplateViewModel : ViewModelBase
    {
        public ReactivePropertySlim<string> Name { get; set; }
        public ReadOnlyReactivePropertySlim<DateTime> Created { get; set; }
        public ReadOnlyReactivePropertySlim<DateTime> Updated { get; set; }
        public ReadOnlyReactivePropertySlim<string> Project { get; set; }
        public TemplateModel Model { get; set; }

        public ReactivePropertySlim<string> InputedName { get; set; }
        public ReactiveCommandSlimToolTip<TemplateNameDialog> OkCommand { get; set; }
        public ReactiveCommandSlim<TemplateNameDialog> CancelCommand { get; set; }

        public TemplateViewModel(TemplateModel model, Func<TemplateViewModel, string, string> validateName)
        {
            Model = model;

            Name = model.ToReactivePropertySlimAsSynchronized(m => m.Name).AddTo(disposables);
            Created = model.ObserveProperty(m => m.Created).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Updated = model.ObserveProperty(m => m.Updated).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Project = model.ObserveProperty(m => m.Target.Ticket.Project.Name).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Name.Pairwise().SubscribeWithErr(p =>
            {
                var err = validateName(this, p.NewItem);
                if (err != null)
                {
                    model.Name = p.OldItem;
                    throw new ApplicationException(err);
                }
            }).AddTo(disposables);

            InputedName = new ReactivePropertySlim<string>(model.Name).AddTo(disposables);
            OkCommand = InputedName.Skip(1).Select(n => validateName(this, n)).ToReactiveCommandToolTipSlim<TemplateNameDialog>().WithSubscribe(dialog =>
            {
                dialog.DialogResult = true;
                dialog.Close();
            }).AddTo(disposables);
            CancelCommand = new ReactiveCommandSlim<TemplateNameDialog>().WithSubscribe(dialog => dialog.Close()).AddTo(disposables);
        }

        public bool ShowInputNameDialog()
        {
            var dialog = new TemplateNameDialog();
            dialog.DataContext = this;
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var r = dialog.ShowDialog();
            if (r.HasValue && r.Value == true)
            {
                Name.Value = InputedName.Value;
                return true;
            }
            else
            {
                // 初期表示は現在の名前にしたいのでもとに戻す
                InputedName.Value = Name.Value;
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is TemplateViewModel vm && Model.Equals(vm.Model);
        }

        public override int GetHashCode()
        {
            return -623947254 + Model.GetHashCode();
        }
    }
}
