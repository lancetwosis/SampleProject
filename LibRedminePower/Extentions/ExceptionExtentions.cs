﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class ExceptionExtentions
    {
        public static string GetDescription(this Exception e)
        {
            var builder = new StringBuilder();

            AddException(builder, e);

            return builder.ToString();
        }

        private static void AddException(StringBuilder builder, Exception e)
        {
            builder.AppendLine($"Message: {e.Message}");
            builder.AppendLine($"Stack Trace: {e.StackTrace}");

            if (e.InnerException != null)
            {
                builder.AppendLine("Inner Exception");
                AddException(builder, e.InnerException);
            }
        }
    }
}
