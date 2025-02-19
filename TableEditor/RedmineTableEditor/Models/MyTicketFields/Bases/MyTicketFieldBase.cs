using LibRedminePower.Extentions;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Converters;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Extentions;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.TicketFields.Bases;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Views;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTableEditor.Models.MyTicketFields.Bases
{
    public abstract class MyTicketFieldBase<T> : ModelBase
    {
        public T Value { get; set; }
        public string ToolTip { get; set; }
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;
    }
}
