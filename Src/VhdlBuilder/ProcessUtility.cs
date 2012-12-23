﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation;
using VhdlBuilder.Representation.Declaration;
using VhdlBuilder.Representation.Expression;

namespace VhdlBuilder
{
    public static class ProcessUtility
    {
        public static void AddClockToProcesses(Module module, string clockSignalName)
        {
            var clockPort = new Port
            {
                Mode = PortMode.In,
                Name = clockSignalName,
                DataType = new DataType { Name = "std_logic" }
            };

            module.Entity.Ports.Add(clockPort);

            foreach (var process in module.Architecture.Body.Where(element => element is Process).Select(element => element as Process))
            {
                process.SesitivityList.Add(clockPort);
                process.Body.Insert(0, new Raw("if rising_edge(" + clockSignalName.ToVhdlId() + ") then "));
                process.Body.Add(new Raw("end if;"));
            }
        }

        //public static void AddAsyncResetToProcesses(Module module, string resetSignalName)
        //{
        //    var resetPort = new Port
        //    {
        //        Mode = PortMode.In,
        //        Name = resetSignalName,
        //        DataType = new DataType { Name = "std_logic" }
        //    };

        //    module.Entity.Ports.Add(resetPort);

        //    foreach (var process in module.Architecture.Body.Where(element => element is Process).Select(element => element as Process))
        //    {
        //        process.SesitivityList.Add(resetPort);
        //        process.Body.Insert(0, new Raw("if " + resetSignalName.ToVhdlId() + " /= '1' then "));
        //        process.Body.Add(new Raw("end if;"));
        //    }

        //    var resetProcess = new Process { Name = "Reset" };
        //    resetProcess.SesitivityList.Add(resetPort);
        //    resetProcess.Body.Add(new Raw(
        //        "if " + resetSignalName.ToVhdlId() + " = '1' then " +
        //        string.Concat(
        //            module.Architecture.Declarations
        //                .Where(declaration => declaration is IDataObject && ((IDataObject)declaration).ObjectType == ObjectType.Signal)
        //                .Select(declaration => ((IDataObject)declaration).Name.ToVhdlId() + " <= " + ) +
        //        "end if;"));
        //    module.Architecture.Body.Add(resetProcess);
        //}
    }
}
