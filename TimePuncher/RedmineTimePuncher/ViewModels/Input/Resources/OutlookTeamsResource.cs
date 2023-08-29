using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RedmineTimePuncher.ViewModels.Input.Resources
{
    public class OutlookTeamsResource : Bases.MyResourceBase
    {
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public BitmapSource Image2 { get; }
        public ResourceUpdater Updater2 { get; set; }

        public OutlookTeamsResource(OutlookManager outlook, TeamsManager teams) 
            : base(Bases.Type.OutlookTeams,
                   string.Join("/", getNameIcons(outlook.IsInstalled, teams.IsInstalled).Select(a => a.name)), 
                   null,
                   (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4040C2"), true)
        {

            if (outlook.IsInstalled)
            {
                var nameIcon = getNameIconsOutlook();
                Name1 = nameIcon.name;
                Image = nameIcon.bitmap.ToBitmapSource();
                Updater.Indicator.ToolTip = string.Format(Properties.Resources.LastUpdateTime, Name1);
                Updater.Indicator.DateTime = Properties.Settings.Default.LastTimeIndicatorOutlook;
            }
            if (teams.IsInstalled)
            {
                var nameIcon = getNameIconsTeams();
                Name2 = nameIcon.name;
                Image2 = nameIcon.bitmap.ToBitmapSource();

                Updater2 = new Bases.ResourceUpdater(this, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.SlateBlue));
                Updater2.Indicator.StrokeDashArray = new System.Windows.Media.DoubleCollection(new[] { 2.0 });
                Updater2.Indicator.ToolTip = string.Format(Properties.Resources.LastUpdateTime, Name2);
                Updater2.Indicator.DateTime = Properties.Settings.Default.LastTimeIndicatorTeams;
                IsBusy = new[]
                {
                    Updater.IsBusy,
                    Updater2.IsBusy,
                }.CombineLatestValuesAreAllFalse().Inverse().ToReadOnlyReactivePropertySlim();
            }
        }
        private static (string name, Bitmap bitmap) getNameIconsOutlook()
        {
            return ("Outlook", Properties.Resources.outlook16);
        }
        private static (string name, Bitmap bitmap) getNameIconsTeams()
        {
            return ("Teams", Properties.Resources.teams16);
        }

        private static List<(string name, Bitmap bitmap)> getNameIcons(bool isInstalledOutlook, bool isInstalledTeams)
        {
            var result = new List<(string, Bitmap)>();
            if (isInstalledOutlook) result.Add(getNameIconsOutlook());
            if (isInstalledTeams) result.Add(getNameIconsTeams());
            return result;
        }

        public override IEnumerable<ResourceUpdater> GetReloads()
        {
            if (Updater != null)
                yield return Updater;
            if (Updater2 != null)
                yield return Updater2;
        }

    }
}
