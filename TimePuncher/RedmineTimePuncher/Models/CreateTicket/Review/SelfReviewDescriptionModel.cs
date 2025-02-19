using LibRedminePower.Enums;
using LibRedminePower.Models;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class SelfReviewDescriptionModel : ModelBaseSlim
    {
        public string Prefix { get; set; }
        public string Postfix { get; set; }

        public SelfReviewDescriptionModel()
        {
        }

        public SelfReviewDescriptionModel(string inputText, string detectProcess, string reviewMethod, string reviewTarget, string showAllPoints, string requestTranscribe)
        {
            if (string.IsNullOrEmpty(inputText))
                inputText = Resources.ReviewPleaseFollwings;

            Prefix = string.Join($"{Environment.NewLine}{Environment.NewLine}",
                new[] { inputText, detectProcess, reviewMethod, reviewTarget }.Where(p => !string.IsNullOrEmpty(p)));

            Postfix = string.Join($"{Environment.NewLine}{Environment.NewLine}",
                new[] { showAllPoints, requestTranscribe }.Where(p => !string.IsNullOrEmpty(p)));
        }

        public string CreateDescription(string createPoint)
        {
            return string.Join($"{Environment.NewLine}{Environment.NewLine}", new[] { Prefix, createPoint, Postfix });
        }
    }
}