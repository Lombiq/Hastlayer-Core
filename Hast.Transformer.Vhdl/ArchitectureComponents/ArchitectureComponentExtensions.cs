using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public static class ArchitectureComponentExtensions
    {
        public static string CreateStartedSignalName(this IMemberStateMachine stateMachine)
        {
            return ArchitectureComponentNameHelper.CreateStartedSignalName(stateMachine.Name);
        }

        public static string CreateFinishedSignalName(this IMemberStateMachine stateMachine)
        {
            return ArchitectureComponentNameHelper.CreateFinishedSignalName(stateMachine.Name);
        }

        public static string CreateReturnVariableName(this IMemberStateMachine stateMachine)
        {
            return ArchitectureComponentNameHelper.CreateReturnVariableName(stateMachine.Name);
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
                component.Signals.Any(signal => signal.Name == component.CreatePrefixedObjectName(objectName)))
            {
                objectName = name + "." + ++objectNameIndex;
            }

            return component.CreatePrefixedObjectName(objectName);
        }

        public static Variable CreateVariableWithNextUnusedIndexedName(
            this IArchitectureComponent component, 
            string name, 
            DataType dataType)
        {
            var returnVariable = new Variable
            {
                Name = component.GetNextUnusedIndexedObjectName(name),
                DataType = dataType
            };

            component.LocalVariables.Add(returnVariable);

            return returnVariable;
        }
    }
}
