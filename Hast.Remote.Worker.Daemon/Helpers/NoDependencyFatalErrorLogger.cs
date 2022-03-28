using Hast.Layer;
using System;
using System.IO;
using System.Text;

namespace Hast.Remote.Worker.Daemon.Helpers;

/// <summary>
/// This helper is used when an exception gets thrown before we get the <see cref="Hastlayer"/> instance, so normal
/// loggers can't be acquired.
/// </summary>
public static class NoDependencyFatalErrorLogger
{
    public static void Log(Exception exception)
    {
        var fileName = FormattableString.Invariant($"hastlayer-log-{DateTime.UtcNow:yyyy-MM-dd}.log");
        var logsPath = Path.Combine(Program.ApplicationDirectory, "App_Data", "logs");

        try
        {
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }
        }
        catch (Exception)
        {
            // ignored
        }

        using var textWriter = CreateFatalLogger(Path.Combine(logsPath, fileName), fileName);
        textWriter.WriteLine("Exception: {0}", exception);
        textWriter.WriteLine();
    }

    private static TextWriter CreateFatalLogger(params string[] paths)
    {
        TextWriter writer = null;
        foreach (var path in paths)
        {
            try
            {
                writer = new StreamWriter(path, append: true, Encoding.UTF8);
                writer.WriteLine("FATAL ERROR BEFORE OR DURING HASTLAYER INSTANCE CREATION!");
                writer.WriteLine("Current Directory: {0}", Directory.GetCurrentDirectory());
                writer.WriteLine(
                    "The {0} file: {1}",
                    Hastlayer.AppsettingsJsonFileName,
                    File.Exists(Hastlayer.AppsettingsJsonFileName)
                        ? "\n" + File.ReadAllText(Hastlayer.AppsettingsJsonFileName)?.Trim()
                        : "doesn't exist.");
                return writer;
            }
            catch
            {
                writer?.Dispose();
            }
        }

        return TextWriter.Null;
    }
}
