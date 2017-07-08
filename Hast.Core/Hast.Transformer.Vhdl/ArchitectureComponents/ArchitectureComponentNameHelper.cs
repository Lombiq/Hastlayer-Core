using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public enum ParameterFlowDirection
    {
        In,
        Out
    }


    public static class ArchitectureComponentNameHelper
    {
        public static string CreateParameterSignalName(string componentName, string parameterName, ParameterFlowDirection direction)
        {
            return CreatePrefixedSegmentedObjectName(componentName, parameterName, NameSuffixes.Parameter, direction.ToString());
        }

        public static string CreateReturnSignalName(string componentName)
        {
            return CreatePrefixedObjectName(componentName, NameSuffixes.Return);
        }

        public static string CreateStartedSignalName(string componentName)
        {
            return CreatePrefixedObjectName(componentName, NameSuffixes.Started);
        }

        public static string CreateFinishedSignalName(string componentName)
        {
            return CreatePrefixedObjectName(componentName, NameSuffixes.Finished);
        }

        public static string CreateIndexedComponentName(string componentName, int index)
        {
            return componentName + "." + index;
        }

        public static string CreatePrefixedSegmentedObjectName(string componentName, params string[] segments)
        {
            return CreatePrefixedObjectName(componentName, string.Join(".", segments));
        }

        public static string CreatePrefixedObjectName(string componentName, string name)
        {
            return CreatePrefixedExtendedVhdlId(componentName, "." + name);
        }

        public static string CreatePrefixedExtendedVhdlId(string componentName, string id)
        {
            return (componentName + id).ToExtendedVhdlId();
        }
    }
}
