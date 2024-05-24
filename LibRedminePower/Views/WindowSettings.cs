using LibRedminePower.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Telerik.Windows.Controls;

namespace LibRedminePower.Views
{
    public class WindowSettings : ApplicationSettingsBase
    {

        [UserScopedSetting]
        public Rect Location
        {
            get
            {
                if (this["Location"] != null)
                    return (Rect)this["Location"];
                else
                    return Rect.Empty;
            }
            set
            {
                this["Location"] = value;
            }
        }

        [UserScopedSetting]
        public WindowState WindowState
        {
            get
            {
                if (this["WindowState"] != null)
                    return (WindowState)this["WindowState"];
                else
                    return WindowState.Normal;
            }
            set
            {
                this["WindowState"] = value;
            }
        }

        [UserScopedSetting]
        public bool IsUpgraded
        {
            get
            {
                return this["IsUpgraded"] != null ? (bool)this["IsUpgraded"] : false;
            }
            set
            {
                this["IsUpgraded"] = value;
            }
        }

        private Window window;

        public WindowSettings(Window window) : base(window.GetType().Name + "WindowSettings")
        {
            this.window = window;
        }

        public void Attach()
        {
            loadWindowState();
            this.window.Closed += (s, e) => saveWindowState();
        }

        private void loadWindowState()
        {
            if (!IsUpgraded)
            {
                Upgrade();
                IsUpgraded = true;
                try
                {
                    Save();
                }
                catch { }
            }

            Reload();

            // 最小化されていたらメイン画面中央に表示
            if (WindowState == WindowState.Minimized)
            {
                setCenter(WindowState.Normal);
                return;
            }

            var setLocation = false;
            if (!Location.IsEmpty)
            {
                try
                {
                    var rectangle = new Rectangle(
                        Convert.ToInt32(Location.Left),
                        Convert.ToInt32(Location.Top),
                        Convert.ToInt32(Location.Width),
                        Convert.ToInt32(Location.Height));

                    // 画面外に出ていないか調査
                    foreach (var sc in Screen.AllScreens)
                    {
                        if (sc.WorkingArea.Contains(rectangle))
                        {
                            this.window.WindowState = WindowState;
                            this.window.Left = Location.Left;
                            this.window.Top = Location.Top;
                            this.window.Width = Location.Width;
                            this.window.Height = Location.Height;
                            setLocation = true;
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }

            if (!setLocation)
            {
                // 位置復元をしなかった場合は、メイン画面中央に表示する
                setCenter(WindowState);
            }
        }

        private void setCenter(WindowState windowState)
        {
            this.window.WindowState = windowState;
            this.window.Left = (Screen.PrimaryScreen.WorkingArea.Width - this.window.Width) / 2;
            this.window.Top = (Screen.PrimaryScreen.WorkingArea.Height - this.window.Height) / 2;
        }

        private void saveWindowState()
        {
            // 最小化されていた場合は保存せず、次回起動時には今回の初期位置を適用する
            if (this.window.WindowState != WindowState.Minimized &&
                this.window.Visibility == Visibility.Visible)
            {
                WindowState = this.window.WindowState;
                Location = new Rect(this.window.Left, this.window.Top, this.window.Width, this.window.Height);
                try
                {
                    Save();
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Failed to save WindowSettings");
                }
            }
        }
    }
}
