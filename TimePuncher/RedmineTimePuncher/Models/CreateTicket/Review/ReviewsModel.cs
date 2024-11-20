using LibRedminePower.Enums;
using LibRedminePower.Models;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class ReviewsModel : ModelBaseSlim
    {
        public int SelectedIndex { get; set; } = 0;
        public ObservableCollection<ReviewModel> Reviews { get; set; } = new ObservableCollection<ReviewModel>();
        public ObservableCollection<TemplateModel> Templates { get; set; } = new ObservableCollection<TemplateModel>();

        public ReviewsModel()
        {
        }
    }
}