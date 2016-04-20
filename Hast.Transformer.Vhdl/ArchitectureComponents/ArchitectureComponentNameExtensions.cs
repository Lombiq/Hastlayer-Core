using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public static class ArchitectureComponentNameExtensions
    {
        public static string CreateStartedSignalName(this IArchitectureComponent component)
        {
            return ArchitectureComponentNameHelper.CreateStartedSignalName(component.Name);
        }

        public static string CreateFinishedSignalName(this IArchitectureComponent component)
        {
            return ArchitectureComponentNameHelper.CreateFinishedSignalName(component.Name);
        }

        public static string CreateParameterVariableName(this IArchitectureComponent component, string parameterName)
        {
            return ArchitectureComponentNameHelper.CreateParameterVariableName(component.Name, parameterName);
        }

        public static string CreateReturnVariableName(this IArchitectureComponent component)
        {
            return ArchitectureComponentNameHelper.CreateReturnVariableName(component.Name);
        }

        public static string CreateReturnVariableNameForTargetComponent(
            this IArchitectureComponent component, 
            string targetMemberName,
            int index)
        {
            return component
                .CreatePrefixedSegmentedObjectName(targetMemberName, NameSuffixes.Return, index.ToString());
        }

        public static string CreatePrefixedSegmentedObjectName(this IArchitectureComponent component, params string[] segments)
        {
            return ArchitectureComponentNameHelper.CreatePrefixedSegmentedObjectName(component.Name, segments);
        }

        /// <summary>
        /// Creates a VHDL object (i.e. signal or variable) name prefixes with the component's name.
        /// </summary>
        public static string CreatePrefixedObjectName(this IArchitectureComponent component, string name)
        {
            return ArchitectureComponentNameHelper.CreatePrefixedObjectName(component.Name, name);
        }

        /// <summary>
        /// Determines the name of the next available name for a VHDL object (i.e. signal or variable) whose name is
        /// suffixed with a numerical index.
        /// </summary>
        /// <example>
        /// If we need a variable with the name "number" then this method will create a name like "ComponentName.number.0",
        /// or if that exists, then the next available variation like "ComponentName.number.5".
        /// </example>
        /// <returns>An object name prefixed with the component's name and suffixed with a numerical index.</returns>
        public static string GetNextUnusedIndexedObjectName(this IArchitectureComponent component, string name)
        {
            var objectName = name + ".0";
            var objectNameIndex = 0;

            while (
                component.LocalVariables.Any(variable => variable.Name == component.CreatePrefixedObjectName(objectName)) ||
                component.InternallyDrivenSignals.Any(signal => signal.Name == component.CreatePrefixedObjectName(objectName)))
            {
                objectName = name + "." + ++objectNameIndex;
            }

            return component.CreatePrefixedObjectName(objectName);
        }
    }
}
