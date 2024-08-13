using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class IdName : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public static int INVALID_ID = -1;

        public int Id { get; set; }
        public string Name{ get; set; }

        public IdName()
        {}

        public IdName(IdentifiableName identifiable)
        {
            if (identifiable != null)
            {
                Id = identifiable.Id;
                Name = identifiable.Name;
            }
        }

        public override string ToString()
        {
            return $"Id={Id}, Name={Name}";
        }

        public IdentifiableName ToIdentifiableName() => new IdentifiableName() { Id = Id, Name = Name };
    }

    public static class IdNameEx
    {
        /// <summary>
        /// idNames が null もしくは空だった場合、ApplicationException 発報。
        /// t が null もしくは t.Id に一致する要素が idNames に存在しなかった場合、idNames の先頭を返す。
        /// </summary>
        public static T FirstOrDefault<T>(this IEnumerable<T> idNames, T t) where T : IdName
        {
            if (idNames == null && !idNames.Any())
                throw new ApplicationException();

            if (t == null)
                return idNames.First();

            var first = idNames.FirstOrDefault(a => a.Id == t.Id);
            return first != null ? first : idNames.First();
        }
    }

}
