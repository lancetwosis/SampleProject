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

namespace LibRedminePower.Views.Controls
{
    public class FilterTextBox : Control
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FilterTextBox), new PropertyMetadata(""));


        public ICommand ClearTextCommand
        {
            get { return (ICommand)GetValue(ClearTextCommandProperty); }
            set { SetValue(ClearTextCommandProperty, value); }
        }
        public static readonly DependencyProperty ClearTextCommandProperty =
            DependencyProperty.Register("ClearTextCommand", typeof(ICommand), typeof(FilterTextBox), new PropertyMetadata());


        static FilterTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterTextBox), new FrameworkPropertyMetadata(typeof(FilterTextBox)));
        }
    }
}
