using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using Orchard.Environment.Configuration;

namespace Hast.Layer
{
    public class Hastlayer : IHastLayer
    {
        private const string ShellName = ShellSettings.DefaultName;

        private IOrchardAppHost _host;


        /*
         * Steps to be implemented:
         * - Transform into hardware definition through ITransformer.
         * - Save hardware definition for re-use (cache file, stream supplied from the outside).
         * - Synthesize hardware through vendor-specific toolchain and load it onto FPGA, together with the necessary communication 
         *   implementation (currently partially implemented with a call chain table).
         * - Cache hardware implementation to be able to re-load it onto the FPGA later.
         */
        public async Task<IHardwareAssembly> GenerateHardware(Assembly assembly)
        {
             var hardwareDefinition = await (await GetHost()).RunGet(scope => scope.Resolve<ITransformer>().Transform(assembly), ShellName, false);
             if (hardwareDefinition.Language == "VHDL")
             {
                 var vhdlHardwareDefinion = (Hast.Transformer.Vhdl.VhdlHardwareDefinition)hardwareDefinition;
                 var vhdl = vhdlHardwareDefinion.Manifest.TopModule.ToVhdl();
                 //System.IO.File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", vhdl);
                 new Hast.Transformer.Vhdl.HardwareRepresentationComposer().Compose(vhdlHardwareDefinion);
             }

            throw new NotImplementedException();
        }


        // Maybe this should return an IDisposable? E.g. close communication to FPGA, or clean up its configuration here if no other calls for
        // this type of object is alive.
        public T GenerateProxy<T>(IHardwareAssembly hardwareAssembly, T hardwareObject)
        {
            /*
             * - Generate dynamic proxy for the given object.
             * - For the type's methods implement: FPGA communication, data transformation.
             */

            if (hardwareAssembly.SoftAssembly != hardwareObject.GetType().Assembly)
            {
                throw new InvalidOperationException("The supplied type is not part of this hardware assembly.");
            }

            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_host == null) return;

            _host.Dispose();
            _host = null;
        }


        private async Task<IOrchardAppHost> GetHost()
        {
            if (_host != null) return _host;

            var settings = new AppHostSettings
            {
                ImportedExtensions = new[] { typeof(Hast.Transformer.ITransformer).Assembly },
                DefaultShellFeatureStates = new[]
                {
                    new DefaultShellFeatureState
                    {
                        ShellName = ShellName,
                        EnabledFeatures = new[] { "Hast.Transformer", "Hast.Transformer.Vhdl" }
                    }
                }
            };

            _host = await OrchardAppHostFactory.StartTransientHost(settings, null, null);

            return _host;
        }
    }
}
