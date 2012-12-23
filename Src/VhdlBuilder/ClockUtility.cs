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
        public static void AddClockSignalToProcesses(Module module, string signalName)
        {
            var clockPort = new Port
            {
                Mode = PortMode.In,
                Name = signalName,
                DataType = new DataType { Name = "std_logic" }
            };

            module.Entity.Ports.Add(clockPort);

            foreach (var process in module.Architecture.Body.Where(element => element is Process).Select(element => element as Process))
            {
                process.SesitivityList.Add(clockPort);
                process.Body.Insert(0, new Raw("if rising_edge(" + signalName.ToVhdlId() + ") then "));
                process.Body.Add(new Raw("end if;"));
            }
        }
    }
}
