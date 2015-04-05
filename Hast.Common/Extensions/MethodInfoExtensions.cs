using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Gets the full name of the method, including the full namespace of the parent type(s) as well as their return type 
        /// and the types of their (type) arguments.
        /// </summary>
        public static string GetFullName(this MethodInfo method)
        {
            return
                method.ReturnType.FullName + " " +
                method.ReflectedType.FullName + "::" +
                method.Name + 
                "(" + string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType.FullName)) + ")";
        }
    }
}
