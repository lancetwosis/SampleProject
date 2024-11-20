using LibRedminePower;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class TemplateModel : ReviewModel
    {
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public TemplateModel()
        {
        }

        public TemplateModel(string name, ReviewModel source)
        {
            Save(source);

            Name = name;
            Created = Updated;
        }

        public void Save(ReviewModel source)
        {
            var clone = source.Clone();
            Target = clone.Target;
            Requests = clone.Requests;

            Updated = DateTime.Now;
        }

        public override bool Equals(object obj)
        {
            return this.JsonEquals(obj);
        }

        public override int GetHashCode()
        {
            return 106246568 + this.GetJsonHashcode();
        }
    }
}