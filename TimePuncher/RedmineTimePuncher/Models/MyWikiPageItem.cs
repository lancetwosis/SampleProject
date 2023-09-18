using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Models
{
    public class MyWikiPageItem : LibRedminePower.Models.Bases.ModelBase
    {
        public string DisplayTitle => IsTopWiki ? $"{Title} ({Properties.Resources.WikiCountTopPage})" : Title;
        public string UrlBase { get; set; }
        public string Url { get; set; }
        public string ProjectId { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int Version { get; set; }
        public IdentifiableName Author { get; set; }
        public string ParentTitle { get; set; }
        public bool IsTopWiki => Title == "Wiki";

        public MyWikiPageItem(MyWikiPageItem my)
        {
            UrlBase = my.UrlBase;
            Url = $"{UrlBase}projects/{my.ProjectId}/wiki/{my.Title}";
            ProjectId = my.ProjectId;
            Title = my.Title;
            CreatedOn = my.CreatedOn;
            UpdatedOn = my.UpdatedOn;
            Author = my.Author;
            Version = my.Version;
            ParentTitle = my.ParentTitle;
        }

        public MyWikiPageItem(string urlBase, string projectId, WikiPage rawWikiPage)
        {
            UrlBase = urlBase;
            Url = $"{UrlBase}projects/{projectId}/wiki/{rawWikiPage.Title}";
            ProjectId = projectId;
            Title = rawWikiPage.Title;
            CreatedOn = rawWikiPage.CreatedOn;
            UpdatedOn = rawWikiPage.UpdatedOn;
            Author = rawWikiPage.Author;
            Version = rawWikiPage.Version;
            ParentTitle = rawWikiPage.ParentTitle;
        }

        public void GoToWiki()
        {
            System.Diagnostics.Process.Start(Url);
        }
    }
}
