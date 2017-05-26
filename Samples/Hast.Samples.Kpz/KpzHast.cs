﻿using Hast.Common.Configuration;
using Hast.Layer;
using Hast.Transformer.Vhdl.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hast.Samples.Kpz
{
    public partial class Kpz
    {
        public string VhdlOutputFilePath = @"Hast_IP.vhd";
        public delegate void LogItDelegate(string toLog);
        public LogItDelegate LogItFunction; //Should be AsyncLogIt from ChartForm
        public KpzKernelsInterface Kernels;

        public async Task InitializeHastlayer()
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
            configuration.HardwareEntryPointMemberNamePrefixes.Add("Hast.Samples.Kpz.KpzKernels");
            configuration.VhdlTransformerConfiguration().VhdlGenerationOptions = 
                Hast.VhdlBuilder.Representation.VhdlGenerationOptions.Debug;

            LogItFunction("Generating hardware...");
            var hardwareRepresentation = await hastlayer.GenerateHardware( new[] {
                typeof(KpzKernelsInterface).Assembly,
             //   typeof(Hast.Algorithms.MWC64X).Assembly
            }, configuration);
            File.WriteAllText(
                VhdlOutputFilePath,
                ((Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareRepresentation.HardwareDescription).VhdlSource
                );

            LogItFunction("Generating proxy...");
            if (kpzTarget == KpzTarget.Fpga)
            {
                Kernels = await hastlayer.GenerateProxy<KpzKernelsInterface>(hardwareRepresentation, new KpzKernelsInterface());
                LogItFunction("FPGA target detected");
            }
            else //if (kpzTarget == KpzTarget.FPGASimulation)
            {
                Kernels = new KpzKernelsInterface();
                LogItFunction("Simulation target detected");
            }

            LogItFunction("Testing FPGA with TestAdd...");
            uint resultFPGA = Kernels.TestAddWrapper(4313,123);
            uint resultCPU  = 4313+123;
            if(resultCPU == resultFPGA) LogItFunction(String.Format("Success: {0} == {1}", resultFPGA, resultCPU));
            else LogItFunction(String.Format("Fail: {0} != {1}", resultFPGA, resultCPU));

            //LogItFunction("Testing FPGA with TestKpzNode...");
            //LogItFunction(String.Format("{0} --> {1}", 2, kpzKernels.TestKpzNodeWrapper(2), 3, kpzKernels.TestKpzNodeWrapper(3)));
            //How to run the same algorithm on the CPU and the FPGA?
            //Run on FPGA:     var output3 = hastlayerOptimizedAlgorithm.Run(9999);
            //Run on CPU:      var cpuOutput = new HastlayerOptimizedAlgorithm().Run(234234);
        }
    }
}
