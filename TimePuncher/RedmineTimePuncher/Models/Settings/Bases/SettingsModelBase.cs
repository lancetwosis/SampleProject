using AutoMapper;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using RedmineTimePuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.Bases
{
    public abstract class SettingsModelBase<T> : LibRedminePower.Models.Bases.ModelBase, ISettingsModel where T : class, new()
    {
        public void Export(string fileName)
        {
            FileHelper.WriteAllText(fileName, this.ToJson());
        }

        public void Import(string fileName)
        {
            var text = System.IO.File.ReadAllText(fileName);
            var newModel = CloneExtentions.ToObject<T>(text);
            var config = new MapperConfiguration(SetupConfigure);
            var mapper = config.CreateMapper();
            mapper.Map(newModel, this);
            return;
        }

        public virtual void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<T, T>();
        }
    }
}
