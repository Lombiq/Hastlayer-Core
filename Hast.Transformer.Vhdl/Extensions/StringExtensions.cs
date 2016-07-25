using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static class StringExtensions
    {
        public static bool IsTaskFromResultMethodName(this string name)
        {
            return name.Contains("System.Threading.Tasks.Task::FromResult");
        }

        public static bool IsTaskCompletedTaskPropertyName(this string name)
        {
            return name.Contains("System.Threading.Tasks.Task::CompletedTask");
        }
    }
}
