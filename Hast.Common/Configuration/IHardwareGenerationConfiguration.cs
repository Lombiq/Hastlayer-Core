using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Configuration
{
    public interface IHardwareGenerationConfiguration
    {
        /// <summary>
        /// Gets or sets the maximal degree of parallelism that will be attempted to build into the generated hardware
        /// when constructs suitable for hardware-level parallelisation are found.
        /// </summary>
        int MaxDegreeOfParallelism { get; set;  }

        /// <summary>
        /// Gets a dictionary that can contain settings for non-default configuration options (like ones required by 
        /// specific transformer implementations).
        /// </summary>
        IDictionary<string, object> CustomConfiguration { get; }

        /// <summary>
        /// Gets the collection of the full name of those public members that will be accessible as hardware 
        /// implementation. By default all members implemented from interfaces and all public virtual members will 
        /// be included.
        /// </summary>
        /// <example>
        /// Specify members with their full name, including the full namespace of the parent types as well as their
        /// return type and the types of their (type) arguments, e.g.:
        /// "System.Boolean Contoso.ImageProcessing.FaceRecognition.FaceDetectors::IsFacePresent(System.Byte[])
        /// </example>
        IEnumerable<string> IncludedMembers { get; }
    }


    // This is a bit more complicated than that, so not supplying such a helper for now.
    //public static class HardwareGenerationConfigurationExtensions
    //{
    //    public static void AddIncludedMethod(this IHardwareGenerationConfiguration configuration, Expression<Func<dynamic>> expression)
    //    {
    //        var methodCallExpression = expression.Body as MethodCallExpression;
    //        if (methodCallExpression == null)
    //        {
    //            throw new InvalidOperationException("The supplied expression is not a method call.");
    //        }
    //        var z = methodCallExpression.Method.Name;
    //    }
    //}
}
