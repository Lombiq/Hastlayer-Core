﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;
using Hast.Tests.TestAssembly1.ComplexTypes;

namespace Hast.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
                {
                    // Generating hardware from samples:
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        hastlayer.Transformed += (sender, e) =>
                            {
                                //File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", ToVhdl(e.HardwareDescription));
                            };

                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly
                                //typeof(Math).Assembly // Would be needed for Math.Sqrt() but transforming that is not yet supported.
                            },
                            HardwareGenerationConfiguration.Default);

                        var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new PrimeCalculator());
                        var isPrime = primeCalculator.IsPrimeNumber(15);
                    }

                    // Generating hardware from test assemblies:
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        hastlayer.Transformed += (sender, e) =>
                        {
                            //File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", ToVhdl(e.HardwareDescription));
                        };

                        var configuration = new HardwareGenerationConfiguration
                        {
                            // Another was would be to add such prefixes (potentially for whole namespaces like here), instead we add a single
                            // method below.
                            //PublicHardwareMemberPrefixes = new[] { "Hast.Tests.TestAssembly1.ComplexTypes.ComplexTypeHierarchy" }
                        };
                        configuration.AddPublicHardwareMethod<IInterface1>(complex => complex.Interface1Method1());

                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(ComplexTypeHierarchy).Assembly
                            }, configuration);

                        // With this interface-typed variable we simulate that the object comes from dependency injection.
                        IInterface1 complexType = new ComplexTypeHierarchy();
                        complexType = await hastlayer.GenerateProxy(hardwareRepresentation, complexType);
                        var output = complexType.Interface1Method1();
                        output = complexType.Interface1Method2();
                    }

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }


        private static string ToVhdl(IHardwareDescription hardwareDescription)
        {
            return ((Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareDescription).Manifest.TopModule.ToVhdl();
        }
    }
}
