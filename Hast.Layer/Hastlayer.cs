﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Communication;
using Hast.Layer.Extensibility.Events;
using Hast.Layer.Models;
using Hast.Synthesis;
using Hast.Synthesis.Services;
using Hast.Transformer;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using Orchard.Environment.Configuration;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Validation;

namespace Hast.Layer
{
    public class Hastlayer : IHastlayer
    {
        private const string ShellName = ShellSettings.DefaultName;

        private readonly IHastlayerConfiguration _configuration;
        private IOrchardAppHost _host;

        public event ExecutedOnHardwareEventHandler ExecutedOnHardware;


        // Private so the static factory should be used.
        private Hastlayer(IHastlayerConfiguration configuration)
        {
            _configuration = configuration;
        }


        public static IHastlayer Create() => Create(HastlayerConfiguration.Default);

        /// <summary>
        /// Instantiates a new <see cref="IHastlayer"/> implementation.
        /// </summary>
        /// <remarks>
        /// Point of this factory is that it returns an interface type instead of the implementation and can throw
        /// exceptions.
        /// </remarks>
        /// <param name="configuration">Configuration for Hastalyer.</param>
        /// <returns>A newly created <see cref="IHastlayer"/> implementation.</returns>
        public static IHastlayer Create(IHastlayerConfiguration configuration)
        {
            Argument.ThrowIfNull(configuration, nameof(configuration));
            Argument.ThrowIfNull(configuration.Extensions, nameof(configuration.Extensions));

            return new Hastlayer(configuration);
        }


        public async Task<IEnumerable<IDeviceManifest>> GetSupportedDevices()
        {
            return await (await GetHost()).RunGet(scope => Task.FromResult(scope.Resolve<IDeviceDriverSelector>().GetSupporteDevices()));
        }

        public async Task<IHardwareRepresentation> GenerateHardware(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration)
        {
            Argument.ThrowIfNull(assemblies, nameof(assemblies));
            if (!assemblies.Any())
            {
                throw new ArgumentException("No assemblies were specified.");
            }

            if (assemblies.Count() != assemblies.Distinct().Count())
            {
                throw new ArgumentException("The same assembly was included multiple times. Only supply each assembly to generate hardware from once.");
            }

            /*
             * Steps to be implemented:
             * - Transform into hardware description through ITransformer.
             * - Save hardware description for re-use (cache file, stream supplied from the outside).
             * - Synthesize hardware through vendor-specific toolchain and load it onto FPGA, together with the necessary 
             *   communication implementation (currently partially implemented with a member table). The implementation
             *   should be cached by the vendor tools.
             */

            try
            {
                HardwareRepresentation hardwareRepresentation = null;

                await (await GetHost())
                    .Run<ITransformer, IHardwareImplementationComposer, IDeviceDriver>(
                        async (transformer, hardwareImplementationComposer, deviceDriver) =>
                        {
                            var hardwareDescription = await transformer.Transform(assemblies, configuration);

                            var hardwareImplementation = await hardwareImplementationComposer.Compose(hardwareDescription);

                            hardwareRepresentation = new HardwareRepresentation
                            {
                                SoftAssemblies = assemblies,
                                HardwareDescription = hardwareDescription,
                                HardwareImplementation = hardwareImplementation,
                                DeviceManifest = deviceDriver.DeviceManifest
                            };
                        }, ShellName, false);

                return hardwareRepresentation;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                var message = "An error happened during generating the Hastlayer hardware representation for the following assemblies: " + string.Join(", ", assemblies.Select(assembly => assembly.FullName));
                await GetHost().Result.Run<ILoggerService>(logger => Task.Run(() => logger.Error(ex, message)));
                throw new HastlayerException(message, ex);
            }
        }

        public async Task<T> GenerateProxy<T>(IHardwareRepresentation hardwareRepresentation, T hardwareObject, IProxyGenerationConfiguration configuration) where T : class
        {
            if (!hardwareRepresentation.SoftAssemblies.Contains(hardwareObject.GetType().Assembly))
            {
                throw new InvalidOperationException("The supplied type is not part of any assembly that this hardware representation was generated from.");
            }

            try
            {
                return await
                    (await GetHost())
                    .RunGet(scope => Task.Run(() => scope.Resolve<IProxyGenerator>().CreateCommunicationProxy(hardwareRepresentation, hardwareObject, configuration)));
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                var message = "An error happened during generating the Hastlayer proxy for an object of the following type: " + hardwareObject.GetType().FullName;
                await GetHost().Result.Run<ILoggerService>(logger => Task.Run(() => logger.Error(ex, message)));
                throw new HastlayerException(message, ex);
            }
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

            var importedExtensions = new[]
                {
                    typeof(Hastlayer).Assembly,
                    typeof(IProxyGenerator).Assembly,
                    typeof(IHardwareImplementationComposer).Assembly,
                    typeof(ITransformer).Assembly
                }.Union(_configuration.Extensions);

            var driversPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var currentDirectory = Path.GetFileName(driversPath);
            if (currentDirectory.Equals("Debug", StringComparison.OrdinalIgnoreCase) ||
                currentDirectory.Equals("Release", StringComparison.OrdinalIgnoreCase))
            {
                driversPath = Path.GetDirectoryName(driversPath);
            }
            currentDirectory = Path.GetFileName(driversPath);
            if (currentDirectory.Equals("bin", StringComparison.OrdinalIgnoreCase))
            {
                driversPath = Path.GetDirectoryName(driversPath);
            }

            driversPath = Path.GetDirectoryName(driversPath); // Now we're at the level above the current project's folder.

            var driversFound = false;
            while (driversPath != null && !driversFound)
            {
                var driversSubFolder = Path.Combine(driversPath, "Drivers");
                if (Directory.Exists(driversSubFolder))
                {
                    driversPath = driversSubFolder;
                    driversFound = true;
                }
                else
                {
                    driversPath = Path.GetDirectoryName(driversPath); 
                }
            }


            var settings = new AppHostSettings
            {
                ImportedExtensions = importedExtensions,
                DefaultShellFeatureStates = new[]
                {
                    new DefaultShellFeatureState
                    {
                        ShellName = ShellName,
                        EnabledFeatures = importedExtensions.Select(extension => extension.ShortName())
                    }
                },
                ModuleFolderPaths = new[] { driversPath, @"E:\Projects\Munka\Lombiq\Hastlayer\Hastlayer" }
            };

            _host = await OrchardAppHostFactory.StartTransientHost(settings, null, null);

            await _host.Run<IHardwareExecutionEventProxy>(proxy => Task.Run(() =>
                proxy.RegisterExecutedOnHardwareEventHandler(eventArgs => ExecutedOnHardware?.Invoke(this, eventArgs))));

            // Enable all loaded features. This is needed so extensions just added to the solution, but not referenced
            // anywhere in the current app can contribute dependencies.
            await (await GetHost())
                .Run<Orchard.Environment.Features.IFeatureManager>(
                    (featureManager) =>
                    {
                        featureManager.EnableFeatures(featureManager.GetAvailableFeatures().Select(feature => feature.Id), true);

                        return Task.CompletedTask;
                    }, ShellName, false);


            return _host;
        }
    }
}
