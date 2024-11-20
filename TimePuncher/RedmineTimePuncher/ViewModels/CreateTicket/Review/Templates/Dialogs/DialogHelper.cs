using RedmineTimePuncher.Views.CreateTicket.Review;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates.Dialogs
{
    public static class DialogHelper
    {
        public static TemplateViewModel SelectTemplate(ObservableCollection<TemplateViewModel> templates, string message)
        {
            using (var vm = new SingleSelectDialogViewModel(templates, message))
            {
                return showDialog(vm) ? vm.SelectedTemplate.Value : null;
            }
        }

        public static List<TemplateViewModel> SelectTemplates(ObservableCollection<TemplateViewModel> templates, string message)
        {
            using (var vm = new MultiSelectDialogViewModel(templates, message))
            {
                return showDialog(vm) ? vm.SelectedTemplates.ToList() : null;
            }
        }

        public static void ShowTemplates(ObservableCollection<TemplateViewModel> templates, string message)
        {
            using (var vm = new ShowListDialogViewModel(templates, message))
            {
                showDialog(vm);
            }
        }

        public static bool ConfirmTemplates(ObservableCollection<TemplateViewModel> templates, string message)
        {
            using (var vm = new ShowListDialogViewModel(templates, message, true))
            {
                return showDialog(vm);
            }
        }

        private static bool showDialog<T>(T t) where T : TemplatesDialogViewModelBase
        {
            var dialog = new TemplatesDialog();
            dialog.DataContext = t;
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var r = dialog.ShowDialog();
            return r ?? false;
        }

    }
}
