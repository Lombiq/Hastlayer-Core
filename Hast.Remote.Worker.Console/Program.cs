using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Hast.Remote.Worker.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Console;

internal static class Program
{
    [SuppressMessage(
        "Globalization",
        "CA1303:Do not pass literals as localized parameters",
        Justification = "This application doesn't need localization.")]
    private static async Task Main()
    {
        var configuration = TransformationWorkerConfiguration.Create();

        var hastlayerConfiguration = await new HastlayerConfigurationProvider().GetConfigurationAsync(configuration);
        using var host = Hastlayer.Create(hastlayerConfiguration);

#if DEBUG
        var logger = host.GetLogger<Hastlayer>(); // It can't be Program becuase that's static.
        for (int i = 0; i < (int)LogLevel.None; i++)
        {
            var logLevel = (LogLevel)i;
            logger.Log(logLevel, "{LogLevel} testing", logLevel.ToString());
        }
#endif

        using var cancellationTokenSource = new CancellationTokenSource();
        System.Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            System.Console.WriteLine("Application cancelled via SIGINT, attempting graceful shutdown.");
            System.Console.WriteLine("Please allow at least 10 seconds for this...");
            cancellationTokenSource.Cancel();
        };

        System.Console.WriteLine("Press Ctrl + C to cleanly terminate the application.");

        await host.RunAsync<ITransformationWorker>(worker => worker.WorkAsync(cancellationTokenSource.Token));
    }
}
