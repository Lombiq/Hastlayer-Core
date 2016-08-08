﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Hast.Common.Configuration
{
    public interface IHardwareGenerationConfiguration
    {
        /// <summary>
        /// Gets a dictionary that can contain settings for non-default configuration options (like ones required by 
        /// specific transformer implementations).
        /// </summary>
        IDictionary<string, object> CustomConfiguration { get; }

        /// <summary>
        /// Gets the collection of the full name of those public members that will be accessible as hardware 
        /// implementation. By default all members implemented from interfaces and all public virtual members will 
        /// be included. You can use this to restrict what gets transformed into hardware; if nothing is specified
        /// all suitable members will be transformed.
        /// </summary>
        /// <example>
        /// Specify members with their full name, including the full namespace of the parent type(s) as well as their
        /// return type and the types of their (type) arguments, e.g.:
        /// "System.Boolean Contoso.ImageProcessing.FaceRecognition.FaceDetectors::IsFacePresent(System.Byte[])
        /// </example>
        IList<string> PublicHardwareMemberFullNames { get; }

        /// <summary>
        /// Gets the collection of the name prefixes of those public members that will be accessible as hardware 
        /// implementation. By default all members implemented from interfaces and all public virtual members will 
        /// be included. You can use this to restrict what gets transformed into hardware; if nothing is specified
        /// all suitable members will be transformed.
        /// </summary>
        /// <example>
        /// Specify members with the leading part of their name as you would access them in C#, e.g.:
        /// "Contoso.ImageProcessing" will include all members under this namespace.
        /// "Contoso.ImageProcessing.FaceRecognition.FaceDetectors" will include all members in this class.
        /// </example>
        IList<string> PublicHardwareMemberNamePrefixes { get; }

        /// <summary>
        /// Gets whether the caching of the generated hardware is allowed. If set to <c>false</c> no caching will happen.
        /// </summary>
        bool EnableCaching { get; }
    }


    public static class HardwareGenerationConfigurationExtensions
    {
        /// <summary>
        /// Adds a public method that will be accessible as hardware implementation.
        /// </summary>
        /// <typeparam name="T">The type of the reference that will be later fed to the proxy generator.</typeparam>
        /// <param name="expression">An expression with a call to the method.</param>
        public static void AddPublicHardwareMethod<T>(this IHardwareGenerationConfiguration configuration, Expression<Action<T>> expression)
        {
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression == null)
            {
                throw new InvalidOperationException("The supplied expression is not a method call.");
            }
            configuration.PublicHardwareMemberFullNames.Add(methodCallExpression.Method.GetFullName());
        }

        // Properties could be added similarly once properties are supported.
    }
}
