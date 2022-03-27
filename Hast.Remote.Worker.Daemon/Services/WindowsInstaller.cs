using CliWrap;
using CliWrap.Buffered;
using Hast.Remote.Worker.Daemon.Constants;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Hast.Remote.Worker.Daemon.Constants.ServiceProperties;

namespace Hast.Remote.Worker.Daemon.Services;

public class WindowsInstaller : IInstaller
{
    public async Task<ExitCode> InstallAsync()
    {
        var executablePath = Path.Combine(Program.ApplicationDirectory, "Hast.Remote.Worker.Daemon.exe");
        var result = await ServiceControlAsync("create", Name, "binpath=" + executablePath, "start=auto");

        return result == ExitCode.Success ? await StartAsync() : result;
    }

    public Task<ExitCode> StartAsync() => ServiceControlAsync("start", Name);

    public Task<ExitCode> StopAsync() => ServiceControlAsync("stop", Name);

    public async Task<ExitCode> UninstallAsync()
    {
        await StopAsync(); // It's okay if this fails, e.g. if the service wasn't even running.
        return await ServiceControlAsync("delete", Name);
    }

    private static async Task<ExitCode> ServiceControlAsync(params string[] arguments)
    {
        Console.WriteLine("sc " + string.Join(" ", arguments));
        var result = await Cli.Wrap("sc")
            .WithArguments(arguments)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        var resultStrings = new[]
            {
                result.StandardOutput,
                result.StandardError,
            }
            .Where(text => !string.IsNullOrWhiteSpace(text));

        Console.WriteLine(string.Join("\n", resultStrings));
        return result.ExitCode == 0 ? ExitCode.Success : ExitCode.ServiceControlError;
    }
}
