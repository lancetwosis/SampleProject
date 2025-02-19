using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Views
{
    public class MyNavigationView : RadNavigationView
    {
        protected override bool HandleKeyDown(Key key)
        {
            // チケット編集のプレビュー機能を実現している WebView2 内の TextArea にて
            // キーボードによるカーソル移動や選択が機能しなくなるため、KeyDown は無視する
            return false;
        }
    }
}
