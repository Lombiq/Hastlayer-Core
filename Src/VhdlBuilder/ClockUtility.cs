using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation;

namespace VhdlBuilder
{
    public static class ClockUtility
    {
        public static void AddClockSignal(Module module, string clkName)
        {
            module.Entity.Ports.Add(new Port
            {
                Mode = PortMode.In,
                Name = clkName,
                DataType = new DataType { Name = "std_logic" }
            });

            foreach (var process in module.Architecture.Body.Where(element => element is Process).Select(element => element as Process))
            {
                process.SesitivityList.Add(clkName);
                process.Body.Insert(0, new Raw { Source = "if " + clkName + "'event and " + clkName + " = '1' then " });
                process.Body.Add(new Raw { Source = "end if;" });
            }
        }
    }
}
