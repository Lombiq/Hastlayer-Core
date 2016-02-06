using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public static class ArchitectureComponentNameHelper
    {
        public static string CreateReturnVariableName(string componentName)
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
