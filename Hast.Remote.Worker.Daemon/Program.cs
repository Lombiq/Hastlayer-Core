﻿using Hast.Remote.Worker.Daemon.Helpers;
using Hast.Remote.Worker.Daemon.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;

namespace Hast.Remote.Worker.Daemon
{
    public static class Program
    {
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
                    })
                    .Build()
                    .Run();
            }
            catch(Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                return 1;
            }

            return 0;
        }
    }
}
