using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Models
{
    [Equals]
    public class MyUser : IdName
    {
        #region "Equalsメソッド" 
        public static bool operator ==(MyUser left, MyUser right) => Operator.Weave(left, right);
        public static bool operator !=(MyUser left, MyUser right) => Operator.Weave(left, right);
        #endregion
        
        public static MyUser NOT_SPECIFIED = new MyUser() { Name = "", Id = INVALID_ID };

        public static string UrlBase;
        public string Email { get; set; }

        [IgnoreDuringEquals]
        public List<Membership> Memberships { get; set; }
        [CustomEqualsInternal]
        bool compareMemberships(MyUser other) => EqualsComparer.AreListsEqual(Memberships, other.Memberships);

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
    }
}
