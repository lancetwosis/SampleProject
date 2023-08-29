using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Attributes
{
    [AttributeUsageAttribute(AttributeTargets.All, Inherited =true, AllowMultiple = true)]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private ResourceManager _recourceManager;
        private string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _recourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string description = _recourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", _resourceKey) : description;
            }
        }
    }
}
