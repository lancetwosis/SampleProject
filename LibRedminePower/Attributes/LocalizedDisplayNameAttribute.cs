using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibRedminePower.Attributes
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string resourceKey, Type resourceType)
            : base(GetMessageFromResource(resourceKey, resourceType))
        {
        }

        private static string GetMessageFromResource(string resourceKey, Type resourceType)
        {
            var manager = new ResourceManager(resourceType);
            string description = manager.GetString(resourceKey);
            return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", resourceKey) : description;
        }
    }
}
