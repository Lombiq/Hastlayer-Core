using Hast.Common.Configuration;
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
        private bool _verifyOutput;

        public async Task InitializeHastlayer(bool verifyOutput)
        {
            _verifyOutput = verifyOutput;

            LogItFunction("Creating Hastlayer Factory...");
            var hastlayer = Xilinx.HastlayerFactory.Create();
            hastlayer.ExecutedOnHardware += (sender, e) =>
            {
                LogItFunction("Hastlayer timer: " +
                    e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds + "ms (net) / " +
                    e.HardwareExecutionInformation.FullExecutionTimeMilliseconds + " ms (total)"
                );
            };

            var configuration = new HardwareGenerationConfiguration();
            configuration.HardwareEntryPointMemberNamePrefixes.Add("Hast.Samples.Kpz.KpzKernels");
            configuration.VhdlTransformerConfiguration().VhdlGenerationOptions = 
                Hast.VhdlBuilder.Representation.VhdlGenerationOptions.Debug;
            configuration.EnableCaching = false;

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
                ProxyGenerationConfiguration proxyConf = new ProxyGenerationConfiguration();
                proxyConf.ValidateHardwareResults = _verifyOutput;
                Kernels = await hastlayer.GenerateProxy<KpzKernelsInterface>(hardwareRepresentation, new KpzKernelsInterface(), proxyConf);
                LogItFunction("FPGA target detected");
            }
            else //if (kpzTarget == KpzTarget.FPGASimulation)
            {
                Kernels = new KpzKernelsInterface();
                LogItFunction("Simulation target detected");
            }

            LogItFunction("Testing FPGA with TestAdd...");
            uint resultFpga = Kernels.TestAddWrapper(4313,123);
            uint resultCpu  = 4313+123;
            if(resultCpu == resultFpga) LogItFunction(String.Format("Success: {0} == {1}", resultFpga, resultCpu));
            else LogItFunction(String.Format("Fail: {0} != {1}", resultFpga, resultCpu));

            if(kpzTarget != KpzTarget.FpgaSimulation)
            {
                LogItFunction("Testing FPGA with TestPrng...");
                uint[] resultPrngFpga = Kernels.TestPrngWrapper();
                KpzKernelsInterface KernelsCpu = new KpzKernelsInterface();
                uint[] resultPrngCpu = KernelsCpu.TestPrngWrapper();
                string numbersFpga = "", numbersCpu = "";
                for (int i = 0; i < 10; i++)
                {
                    numbersCpu += resultPrngCpu[i].ToString() + "";
                    numbersFpga += resultPrngFpga[i].ToString() + "";
                }
                Console.WriteLine("TestPrng CPU results: " + numbersCpu);
                Console.WriteLine("TestPrng FPGA results: " + numbersFpga);
                LogItFunction("Done.");
            }

        }
    }
}
