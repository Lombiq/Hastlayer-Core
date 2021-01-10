using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Hast.Xilinx
{
    public static class ResourceHelper
    {
        public static string GetTimingReport(string csvName)
        {
            var provider = new EmbeddedFileProvider(typeof(AlveoU250Driver).Assembly);
            using var stream = provider.GetFileInfo($"TimingReports.{csvName}.csv").CreateReadStream();
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
