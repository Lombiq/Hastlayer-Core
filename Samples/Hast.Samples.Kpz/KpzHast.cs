using System;
using System.IO;
using Hast.Common.Configuration;
using Hast.Transformer.Vhdl.Configuration;
using Hast.Layer;
using Hast.VhdlBuilder.Representation;

namespace Hast.Samples.Kpz
{
    public partial class Kpz
    {
        public string VhdlOutputFilePath = @"Hast_IP.vhd";
        public delegate void LogItDelegate(string toLog);
        public LogItDelegate LogItFunction; //Should be AsyncLogIt from ChartForm

        private void InitializeHastlayer()
        {
            LogItFunction("Creating Hastlayer Factory...");
            var hastlayer = Xilinx.HastlayerFactory.Create();
            hastlayer.ExecutedOnHardware += (sender, e) =>
            {
                LogItFunction(
                    "Executing " + e.MemberFullName + " on hardware took " +
                    e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds + "ms (net) " +
                    e.HardwareExecutionInformation.FullExecutionTimeMilliseconds + " milliseconds (all together)"
                );
            };
            var configuration = new HardwareGenerationConfiguration();
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.Kpz.KpzKernels");
            configuration.VhdlTransformerConfiguration().VhdlGenerationOptions = 
                Hast.VhdlBuilder.Representation.VhdlGenerationOptions.Debug;

            #region Hack
            // You just had to open this region, didn't you?
            // If VHDL is written to a file just after GenerateHardware() somehow an exception will be
            // thrown from inside Hastlayer. This shouldn't happen and the exception has nothing to do with
            // files. It's a mystery. But having this dummy file write here solves it.
            System.IO.File.WriteAllText(VhdlOutputFilePath, "dummy");
            #endregion

            LogItFunction("Generating hardware...");
            var taskToGenerateHardware = hastlayer.GenerateHardware( new[] { typeof(KpzKernels).Assembly }, configuration);
            taskToGenerateHardware.Start();
            taskToGenerateHardware.Wait();
            var hardwareRepresentation = taskToGenerateHardware.Result;
            File.WriteAllText(
                VhdlOutputFilePath,
                ((Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareRepresentation.HardwareDescription).VhdlSource
                );

            LogItFunction("Generating proxy...");
            KpzKernels kpzKernels;
            if (kpzTarget == KpzTarget.FPGA)
            {
                var taskToGenerateProxy = hastlayer.GenerateProxy<KpzKernels>(hardwareRepresentation, new KpzKernels());
                taskToGenerateProxy.Start();
                taskToGenerateProxy.Wait();
                kpzKernels = taskToGenerateProxy.Result;
                LogItFunction("FPGA target detected");
            }
            else if (kpzTarget == KpzTarget.FPGASimulation)
            {
                kpzKernels = new KpzKernels();
                LogItFunction("Simulation target detected");
            }

            LogItFunction("Testing FPGA with TestAdd...");
            uint resultFPGA = kpzKernels.TestAddWrapper(4313,123);
            uint resultCPU  = 4313+123;
            if(resultCPU == resultFPGA) LogItFunction(String.Format("Success: {0} == {1}", resultFPGA, resultCPU));
            else LogItFunction(String.Format("Fail: {0} != {1}", resultFPGA, resultCPU));
            //How to run the same algorithm on the CPU and the FPGA?
            //Run on FPGA:     var output3 = hastlayerOptimizedAlgorithm.Run(9999);
            //Run on CPU:      var cpuOutput = new HastlayerOptimizedAlgorithm().Run(234234);
        }
    }
}
