using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.ViewModels.Input.Controls;
using RedmineTimePuncher.ViewModels.Input.Slots;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Resources.Bases
{
    public abstract class MyResourceBase : Resource, IDisposable
    {
        public Type Type { get; set; }
        public BitmapSource Image { get; protected set; }
        public ResourceUpdater Updater { get; set; }
        public ResourceSlot Slot { get; }
        public System.Windows.Media.Color Color { get; }
        public System.Windows.Media.SolidColorBrush Brush { get; }
        public bool IsBackground { get; }
        public ReadOnlyReactivePropertySlim<bool> IsBusy { get; set; }
        public ResourceSettingViewModel ResourceSetting { get; set; }

        [NonSerialized]
        protected CompositeDisposable disposables = new CompositeDisposable();

        public MyResourceBase(Type type, string name, string dispalyName, Bitmap image, System.Windows.Media.Color color, bool isBackground) : base(name)
        {
            Type = type;
            DisplayName = dispalyName;
            Image = image?.ToBitmapSource();

            Color = color;
            Brush = new System.Windows.Media.SolidColorBrush(Color);
            IsBackground = isBackground;

            Slot = new ResourceSlot(DateTime.MinValue, DateTime.MaxValue, this);
            Updater = new Bases.ResourceUpdater(this, Brush);
            IsBusy = Updater.IsBusy.ToReadOnlyReactivePropertySlim();
        }

        public MyResourceBase(Type type, string dispalyName, Bitmap image, System.Windows.Media.Color color, bool isBackground)
            : this(type, type.ToString(), dispalyName, image, color, isBackground)
        {
        }

        public virtual IEnumerable<ResourceUpdater> GetReloads()
        {
            if (Updater != null) yield return Updater;
        }

        public bool IsMyWorks()
        {
            return Type == Type.MyWorks;
        }

        public bool IsMembers()
        {
            return Type == Type.Members;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }

    public enum Type
    {
        MyWorks,
        Redmine,
        OutlookTeams,
        Members
    }
}
