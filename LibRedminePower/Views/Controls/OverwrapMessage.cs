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

    public class OverwrapMessage : ContentControl
    {
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(OverwrapMessage), new PropertyMetadata(null));

        static OverwrapMessage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OverwrapMessage), new FrameworkPropertyMetadata(typeof(OverwrapMessage)));
        }
    }
}
