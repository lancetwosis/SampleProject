using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Models
{
    public class MyUser : IdName
    {
        public static MyUser NOT_SPECIFIED = new MyUser() { Name = "", Id = INVALID_ID };

        public static string UrlBase;
        public string Email { get; set; }
        public List<Membership> Memberships { get; set; }

        public MyUser() { }

        public MyUser(User user)
        {
            Id = user.Id;
            Name = $"{user.LastName} {user.FirstName}";
            Email = user.Email;
            Memberships = user.Memberships != null ? user.Memberships : new List<Membership>();
        }

        public static string GetUrl(int id)
        {
            return $"{UrlBase}users/{id}";
        }

        public override string ToString() => Name;

        public override bool Equals(object obj)
        {
            return obj is MyUser user &&
                   Id == user.Id &&
                   Name == user.Name &&
                   Email == user.Email &&
                   (
                       // TODO 以下のチケットで恒久対応を実施する
                       // http://133.242.159.37/issues/1663
                       (Memberships == null && user.Memberships == null) ||
                       (Memberships == null && user.Memberships?.Count == 0) ||
                       (Memberships?.Count == 0 && user.Memberships == null) ||
                        Memberships?.ToJson() == user.Memberships?.ToJson()
                    );
        }

        public override int GetHashCode()
        {
            int hashCode = -351216349;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Email);
            return hashCode;
        }
    }
}
