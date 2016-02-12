using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public static class ArchitectureComponentVariableExtensions
    {
        public static IEnumerable<ParameterVariable> GetParameterVariables(this IArchitectureComponent component)
        {
            return component.GlobalVariables.Where(variable => variable is ParameterVariable).Cast<ParameterVariable>();
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
