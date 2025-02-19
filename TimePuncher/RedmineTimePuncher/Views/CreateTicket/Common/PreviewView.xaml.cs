using Microsoft.Web.WebView2.Core;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.ViewModels.CreateTicket.Review;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.DragDrop;

namespace RedmineTimePuncher.Views.CreateTicket.Common
{
    /// <summary>
    /// PreviewView.xaml の相互作用ロジック
    /// </summary>
    public partial class PreviewView : UserControl
    {
        public PreviewView()
        {
            this.DataContextChanged += (s, e) =>
            {
                var vm = e.NewValue as IPreviewViewModel;
                if (vm != null)
                    vm.WebView2.Value = this.webView2;
            };

            InitializeComponent();
        }
    }
}
