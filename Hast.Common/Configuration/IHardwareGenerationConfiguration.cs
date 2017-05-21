using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        /// Gets the custom configuration if it exists or creates and adds it if it doesn't.
        /// </summary>
        /// <typeparam name="T">Type of the configuration object.</typeparam>
        /// <param name="key">Key where the custom configuration object is stored in the 
        /// <see cref="IHardwareGenerationConfiguration"/> instance.</param>
        /// <returns>The existing or newly created configuration object.</returns>
        public static T GetOrAddCustomConfiguration<T>(this IHardwareGenerationConfiguration hardwareConfiguration, string key)
            where T : new()
        {
            object config;

            if (hardwareConfiguration.CustomConfiguration.TryGetValue(key, out config))
            {
                return (T)config;
            }

            return (T)(hardwareConfiguration.CustomConfiguration[key] = new T());
        }

        /// <summary>
        /// Adds a public method that will be accessible as hardware implementation.
        /// </summary>
        /// <typeparam name="T">The type of the object that will be later fed to the proxy generator.</typeparam>
        /// <param name="expression">An expression with a call to the method.</param>
        public static void AddPublicHardwareMethod<T>(
            this IHardwareGenerationConfiguration configuration, 
            Expression<Action<T>> expression) =>
            configuration.PublicHardwareMemberFullNames.Add(expression.GetMethodFullName());

        /// <summary>
        /// Adds a public type the suitable methods of which will be accessible as hardware implementation.
        /// </summary>
        /// <typeparam name="T">The type of the object that will be later fed to the proxy generator.</typeparam>
        public static void AddPublicHardwareType<T>(this IHardwareGenerationConfiguration configuration) =>
            configuration.PublicHardwareMemberNamePrefixes.Add(typeof(T).FullName);

        // Properties could be added similarly once properties are supported for direct hardware invocation. This is
        // unlikely (since it wouldn't be of much use), though properties inside the generated hardware is already
        // supported.
    }
}
