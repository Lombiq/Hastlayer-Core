﻿using Hast.Layer;
using Hast.Remote.Worker.Daemon.Constants;
using Hast.Remote.Worker.Daemon.Helpers;
using Hast.Remote.Worker.Daemon.Services;
using Hast.Remote.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;

namespace Hast.Remote.Worker.Daemon
{
    public static class Program
    {
        public static ExitCode ExitCode { get; set; }

        public static int Main(string[] args)
        {
            try
            {
                var builder = Host.CreateDefaultBuilder(args);

                builder = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? builder.UseSystemd()
                    : builder.UseWindowsService();

                builder.ConfigureServices((_, services) =>
                    {
                        services.AddSingleton<IEventLogger, EventLogger>();
                        services.AddHostedService<Services.Worker>();
                        Hastlayer.ConfigureLogging(
                            services,
                            TransformationWorkerHastlayerConfigurationProvider.ConfigureLogging);
                    })
                    .Build()
                    .Run();
            }
            catch (OperationCanceledException)
            {
                // Nothing to do here.
            }
            catch(Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                ExitCode = ExitCode.StartupException;
            }

            return (int)ExitCode;
        }
    }
}
