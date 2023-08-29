using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Views.Settings
{
    /// <summary>
    /// RedmineSettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class RedmineSettingsView : UserControl
    {
        public RedmineSettingsView()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                var vm = e.NewValue as ViewModels.Settings.SettingsViewModel;
                this.passwordBox.Password = vm.Redmine.Password.Value;
                this.passwordBoxOfBasic.Password = vm.Redmine.PasswordOfBasicAuth.Value;
            };
        }

        private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as RadPasswordBox;
            var vm = this.DataContext as ViewModels.Settings.SettingsViewModel;
            vm.Redmine.Password.Value = passwordBox.Password;
        }

        private void passwordBoxOfBasic_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as RadPasswordBox;
            var vm = this.DataContext as ViewModels.Settings.SettingsViewModel;
            vm.Redmine.PasswordOfBasicAuth.Value = passwordBox.Password;
        }
    }
}
