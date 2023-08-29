using System.Diagnostics;
using System.Windows.Navigation;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Views
{
    /// <summary>
    /// VersionDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionDialog : RadWindow
    {
        public VersionDialog()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
