using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public static class ArchitectureComponentDataObjectExtensions
    {
        public static IEnumerable<TypedDataObject> GetAllDataObjects(this IArchitectureComponent component)
        {
            return component.GlobalVariables
                .Cast<TypedDataObject>()
                .Union(component.LocalVariables)
                .Union(component.InternallyDrivenSignals)
                .Union(component.ExternallyDrivenSignals);
        }

        public static IEnumerable<Signal> GetAllSignals(this IArchitectureComponent component)
        {
            return component.InternallyDrivenSignals
                .Union(component.ExternallyDrivenSignals);
        }

        public static IEnumerable<Variable> GetAllVariables(this IArchitectureComponent component)
        {
            return component.GlobalVariables
                .Union(component.LocalVariables);
        }

        public static IEnumerable<ParameterSignal> GetParameterSignals(this IArchitectureComponent component)
        {
            return component.GetAllSignals().Where(signal => signal is ParameterSignal).Cast<ParameterSignal>();
        }

        public static Variable CreateVariableWithNextUnusedIndexedName(
            this IArchitectureComponent component,
            string name,
            DataType dataType)
        {
            var variable = new Variable
            {
                Name = component.GetNextUnusedIndexedObjectName(name),
                DataType = dataType
            };

            component.LocalVariables.Add(variable);

            return variable;
        }

        public static Signal CreateSignalWithNextUnusedIndexedName(
            this IArchitectureComponent component,
            string name,
            DataType dataType)
        {
            var signal = new Signal
            {
                Name = component.GetNextUnusedIndexedObjectName(name),
                DataType = dataType
            };

            component.InternallyDrivenSignals.Add(signal);

            return signal;
        }

        public static DataObjectReference CreateParameterSignalReference(this IArchitectureComponent component, string parameterName)
        {
            return ArchitectureComponentNameHelper
                .CreateParameterSignalName(component.Name, parameterName)
                .ToVhdlSignalReference();
        }

        public static DataObjectReference CreateReturnSignalReference(this IArchitectureComponent component)
        {
            return ArchitectureComponentNameHelper
                .CreateReturnSignalName(component.Name)
                .ToVhdlSignalReference();
        }

        public static DataObjectReference CreateReturnSignalReferenceForTargetComponent(
            this IArchitectureComponent component,
            string targetMemberName,
            int index)
        {
            return component
                .CreatePrefixedSegmentedObjectName(targetMemberName, NameSuffixes.Return, index.ToString())
                .ToVhdlSignalReference();
        }
    }
}
