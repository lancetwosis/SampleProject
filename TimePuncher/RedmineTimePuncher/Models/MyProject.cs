using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls.ChartView;

namespace RedmineTimePuncher.Models
{
    public class MyProject : IdName
    {
        public string Identifier { get; set; }

        public List<Redmine.Net.Api.Types.Version> Versions { get; set; } = new List<Redmine.Net.Api.Types.Version>();

        public static List<Brush> COLORS { get; } = ChartPalettes.Windows8.GlobalEntries.Select(a => a.Fill).ToList();

        public MyProject() { }

        public MyProject(Project rawProject) : base(rawProject)
        {
            Identifier = rawProject.Identifier;
        }

        public MyProject(Project rawProject, List<Redmine.Net.Api.Types.Version> versions) : this(rawProject)
        {
            Versions = versions;
        }

        public string CreateVersionLabel(int versionId)
        {
            return $"{Versions.First(v => v.Id == versionId).Name} - {Name}";
        }

        public long CreateVersionValue(int versionId)
        {
            // 「Project の Id」→「Version の Id」の順番で Version のソートが行われるようにする
            return Id * 100000 + versionId;
        }

        public override bool Equals(object obj)
        {
            return obj is MyProject project &&
                   Id == project.Id &&
                   Identifier == project.Identifier;
        }

        public override int GetHashCode()
        {
            int hashCode = -2144243362;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identifier);
            return hashCode;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
