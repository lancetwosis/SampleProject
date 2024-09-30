using LibRedminePower.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Resources
{
    public  class MemberResource : Bases.MyResourceBase
    {
        public MyUser User { get; set; }

        public MemberResource(MyUser user, MyWorksResource myWorks)
            : base(Bases.Type.Members, user?.Id.ToString(), user.Name, null, Colors.Green, true)
        {
            this.User = user;

            IsBusy = myWorks.IsBusy.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Updater = null;
        }
    }
}
