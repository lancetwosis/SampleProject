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
    /// TwinListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class TwinListBox : UserControl
    {
        public DataTemplate FromItemTemplate
        {
            get { return (DataTemplate)GetValue(FromItemTemplateProperty); }
            set { SetValue(FromItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty FromItemTemplateProperty =
            DependencyProperty.Register("FromItemTemplate", typeof(DataTemplate), typeof(TwinListBox), new PropertyMetadata());

        public DataTemplate ToItemTemplate
        {
            get { return (DataTemplate)GetValue(ToItemTemplateProperty); }
            set { SetValue(ToItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ToItemTemplateProperty =
            DependencyProperty.Register("ToItemTemplate", typeof(DataTemplate), typeof(TwinListBox), new PropertyMetadata());

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(TwinListBox), new PropertyMetadata());

        public TwinListBox()
        {
            InitializeComponent();
        }
    }
}
