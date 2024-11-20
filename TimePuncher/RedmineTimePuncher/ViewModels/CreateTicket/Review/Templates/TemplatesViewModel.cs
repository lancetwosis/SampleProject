using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using ObservableCollectionSync;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using RedmineTimePuncher.Views.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates.Dialogs;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates
{
    public class TemplatesViewModel : ViewModelBase
    {
        public ObservableCollectionSync<TemplateViewModel, TemplateModel> Templates { get; set; }
        public ReadOnlyReactivePropertySlim<string> HasTemplate { get; set; }

        public TemplatesViewModel(ObservableCollection<TemplateModel> templates)
        {
            Templates = new ObservableCollectionSync<TemplateViewModel, TemplateModel>(templates,
                m => m != null ? new TemplateViewModel(m, validateName) : null,
                vm => vm?.Model).AddTo(disposables);

            HasTemplate = Templates.AnyAsObservable().Select(a => a ? null : Resources.ReviewTemplateMsgNotExist)
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void Save(ReviewViewModel review)
        {
            if (Templates.IsEmpty())
            {
                SaveAs(review);
            }
            else
            {
                var template = DialogHelper.SelectTemplate(Templates, Resources.ReviewTemplateMsgSelectSave);
                if (template != null)
                    template.Model.Save(review.Model);
            }
        }

        public void SaveAs(ReviewViewModel review)
        {
            var template = new TemplateViewModel(new TemplateModel(review.Name.Value, review.Model), validateName).AddTo(disposables);
            var result = template.ShowInputNameDialog();
            if (result)
                Templates.Add(template);
        }

        private string validateName(TemplateViewModel self, string after)
        {
            if (string.IsNullOrEmpty(after))
                return Resources.ReviewTemplateMsgNameIsEmpty;

            var sameName = Templates.Where(t => t != self && t.Name.Value == after).FirstOrDefault();
            if (sameName != null)
                return Resources.ReviewTemplateMsgSameName;

            return null;
        }

        public void ShowList()
        {
            DialogHelper.ShowTemplates(Templates, Resources.ReviewTemplateMsgList);
        }

        public void Export()
        {
            var targets = DialogHelper.SelectTemplates(Templates, Resources.ReviewTemplateMsgSelectExport);
            if (targets == null)
                return;

            var saveFile = new SaveFileDialog();
            saveFile.FileName = "review_templates";
            saveFile.Filter = "Text Files|*.txt" + "|All Files|*.*";
            saveFile.FilterIndex = 0;
            if (saveFile.ShowDialog().Value == false)
                return;

            try
            {
                FileHelper.WriteAllText(saveFile.FileName, targets.Select(t => t.Model).ToList().ToJson());
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format(Resources.errExport, ex.Message), ex);
            }
        }

        public void Import()
        {
            var openFile = new OpenFileDialog();
            openFile.FileName = "review_templates";
            openFile.Filter = "Text Files|*.txt" + "|All Files|*.*";
            openFile.FilterIndex = 0;
            if (openFile.ShowDialog().Value == false)
                return;

            try
            {
                var models = FileHelper.ReadAllText<List<TemplateModel>>(openFile.FileName);
                var templates = new ObservableCollection<TemplateViewModel>(
                    models.Select(t => new TemplateViewModel(t, validateName).AddTo(disposables)));

                var isOk = DialogHelper.ConfirmTemplates(templates, Resources.ReviewTemplateMsgConfirmImport);
                if (!isOk)
                    return;

                foreach (var t in templates)
                {
                    var err = validateName(null, t.Name.Value);
                    if (err != null)
                    {
                        t.Name.Value = createModifiedName(t.Name.Value);
                    }
                    Templates.Add(t);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format(Resources.errImport, ex.Message), ex);
            }
        }

        private string createModifiedName(string defaultName)
        {
            var format = defaultName + " ({0})";

            var index = 1;
            var modified = string.Format(format, index);
            var sameName = Templates.FirstOrDefault(t => t.Name.Value == modified);
            while (sameName != null)
            {
                index++;
                modified = string.Format(format, index);
                sameName = Templates.FirstOrDefault(t => t.Name.Value == modified);
            }

            return modified;
        }
    }
}
