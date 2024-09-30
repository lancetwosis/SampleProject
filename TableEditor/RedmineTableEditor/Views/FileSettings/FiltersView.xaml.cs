using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace RedmineTableEditor.Views.FileSettings
{
    /// <summary>
    /// FiltersView.xaml の相互作用ロジック
    /// </summary>
    public partial class FiltersView : UserControl
    {
        public FiltersView()
        {
            InitializeComponent();
        }

        private void multiItemsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as RadComboBox;
            var popup = comboBox.ChildrenOfType<Popup>().FirstOrDefault();
            if (popup != null)
            {
                // 選択肢を表示するポップアップの位置を追従させる
                Dispatcher.BeginInvoke((Action)(() => {
                    popup.VerticalOffset -= 1;
                    popup.VerticalOffset += 1;
                }), DispatcherPriority.ApplicationIdle);
            }
        }
    }
}
