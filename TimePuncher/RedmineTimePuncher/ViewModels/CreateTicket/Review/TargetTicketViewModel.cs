using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class TargetTicketViewModel : TargetTicketViewModelBase<TargetTicketModel>
    {
        public DetectionProcessViewModel Process { get; set; }

        public TargetTicketViewModel(TargetTicketModel model) : base(model, Resources.ReviewStatusUnderReview)
        {
            Process = new DetectionProcessViewModel(model).AddTo(disposables);
        }

        public override void Clear()
        {
            base.Clear();

            Process.Clear();
        }

        public MyCustomFieldPossibleValue GetProcess()
        {
            return Process.Setting.Value.IsEnabled ?
                Process.SelectedValue.Value :
                TranscribeSettingModel.NOT_SPECIFIED_PROCESS;
        }

        public void ApplyTemplate(TargetTicketModel template)
        {
            var status = Statuss.Value.FirstOrDefault(s => s.Id == template.StatusUnderRequest.Id);
            if (status != null)
            {
                StatusUnderRequest.Value = status;
            }

            Process.ApplyTemplate(template.Process);
        }
    }
}
