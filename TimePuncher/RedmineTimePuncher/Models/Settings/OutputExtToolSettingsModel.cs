using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{

    public class OutputExtToolSettingsModel : LibRedminePower.Models.Bases.ModelBase
    {
        public string FileName { get; set; }
        public string Argument { get; set; }

        public ReadOnlyReactivePropertySlim<string> Error { get; }

        public OutputExtToolSettingsModel()
        {
            Error = this.ObserveProperty(a => a.FileName).Select(a =>
            {
                if (string.IsNullOrEmpty(FileName))
                    return Properties.Resources.SettingsExpoMsgNeedsExternalToolSetup;
                else if (!System.IO.File.Exists(FileName))
                    return Properties.Resources.SettingsExpoMsgExternalToolNotExist;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
