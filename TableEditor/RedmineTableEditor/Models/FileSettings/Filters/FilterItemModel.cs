using LibRedminePower.Models;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings.Filters
{
    public class FilterItemModel : ModelBaseSlim
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public FilterItemModel()
        {
        }

        public FilterItemModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public FilterItemModel(IdentifiableName identifiable) : this(identifiable.Id.ToString(), identifiable.Name)
        {
        }

        public FilterItemModel(int id, string name) : this(id.ToString(), name)
        {
        }

        public override string ToString() => Name;

        public override bool Equals(object obj)
        {
            return obj is FilterItemModel item &&
                   Id == item.Id &&
                   Name == item.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1919740922;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
