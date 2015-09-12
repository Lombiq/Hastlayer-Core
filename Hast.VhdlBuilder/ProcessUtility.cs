using System.Collections.Generic;
using System.Linq;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder
{
    public static class ProcessUtility
    {
        public static void AddClockToProcesses(Module module, string clockSignalName)
        {
            var clockPort = new Port
            {
                Mode = PortMode.In,
                Name = clockSignalName,
                DataType = KnownDataTypes.StdLogic
            };

            module.Entity.Ports.Add(clockPort);

            foreach (var process in module.Architecture.Body.Where(element => element is Process).Select(element => element as Process))
            {
                process.SesitivityList.Add(clockPort);
                var invokation = new Invokation { Target = "rising_edge".ToVhdlIdValue() };
                invokation.Parameters.Add(clockSignalName.ToVhdlIdValue());
                var wrappingIf = new IfElse
                {
                    Condition = invokation,
                    True = new InlineBlock { Body = new List<IVhdlElement>(process.Body) } // Needs to copy the list.
                };
                process.Body.Clear();
                process.Body.Add(wrappingIf);
            }
        }
    }
}
