using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Remote.Bridge.Models.Api;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Extensions;

namespace Hast.Remote.Client
{
    public class RemoteTransformer : ITransformer
    {
        public async Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration)
        {
            var assemblyContainers = assemblyPaths
                .Select(path => new AssemblyContainer { FileContent = File.ReadAllBytes(path) });
            var apiConfiguration = new Bridge.Models.Api.HardwareGenerationConfiguration
            {
                CustomConfiguration = configuration.CustomConfiguration,
                DeviceName = configuration.DeviceName,
                HardwareEntryPointMemberFullNames = configuration.HardwareEntryPointMemberFullNames,
                HardwareEntryPointMemberNamePrefixes = configuration.HardwareEntryPointMemberNamePrefixes
            };

            var transformationTicket = await ApiClientFactory
                .CreateApiClient(new HastlayerRemoteClientConfiguration { AppId = "TestApp", AppSecret = "appsecret" })
                .TransformAssmblies(new TransformationRequest
                {
                    Assemblies = assemblyContainers,
                    Configuration = apiConfiguration
                });

            var transformationResult = new TransformationResult();

            if (transformationResult.Errors.Any())
            {
                throw new InvalidOperationException(
                    "Transforming the following assemblies failed: " + string.Join(", ", assemblyPaths) +
                    ". The following error(s) happened: " + string.Join(Environment.NewLine, transformationResult.Errors));
            }

            return new RemoteHardwareDescription
            {
                HardwareEntryPointNamesToMemberIdMappings = transformationResult.HardwareDescription.HardwareEntryPointNamesToMemberIdMappings,
                Language = transformationResult.HardwareDescription.Language,
                Source = transformationResult.HardwareDescription.Source
            };
        }


        private class RemoteHardwareDescription : IHardwareDescription
        {
            public IReadOnlyDictionary<string, int> HardwareEntryPointNamesToMemberIdMappings { get; set; }
            public string Language { get; set; }
            public string Source { get; set; }


            public Task WriteSource(Stream stream)
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    // WriteAsync would throw a "The stream is currently in use by a previous operation on the stream." for
                    // FileStreams, even though supposedly there's no operation on the stream.
                    streamWriter.Write(Source);
                }

                return Task.CompletedTask;
            }
        }
    }
}
