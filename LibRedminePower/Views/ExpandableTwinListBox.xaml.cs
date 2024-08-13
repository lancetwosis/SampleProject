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

namespace LibRedminePower.Views
{
    /// <summary>
    /// ExpandableTwinListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ExpandableTwinListBox : UserControl
    {
        public DataTemplate ToItemTemplate
        {
            get { return (DataTemplate)GetValue(ToItemTemplateProperty); }
            set { SetValue(ToItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ToItemTemplateProperty =
            DependencyProperty.Register("ToItemTemplate", typeof(DataTemplate), typeof(ExpandableTwinListBox), new PropertyMetadata());

        public ExpandableTwinListBox()
        {
            InitializeComponent();
        }

        private void twinListGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ラベルの ItemsControl に TwinListBox 全体の幅から「＋」ボタンの幅を除いたものを設定する
            // MaxWidth の指定を行わないと WrapPanel が機能せずすべて一列に表示されてしまう
            this.labelItemsControl.MaxWidth = this.twinListGrid.ActualWidth - 35;
        }
    }
}
