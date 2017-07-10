using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Remote.Bridge.Models;
using Hast.Remote.Worker.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Orchard.Services;

namespace Hast.Remote.Worker
{
    public class TransformationWorker : ITransformationWorker, IDisposable
    {
        private readonly IJsonConverter _jsonConverter;

        private IHastlayer _hastlayer;
        private CloudBlobContainer _container;
        private List<Task> _transformationTasks = new List<Task>();


        public TransformationWorker(IJsonConverter jsonConverter)
        {
            _jsonConverter = jsonConverter;
        }


        public async Task Work(ITransformationWorkerConfiguration configuration, CancellationToken cancellationToken)
        {
            if (_hastlayer == null)
            {
                _hastlayer = await Hastlayer.Create();

                _container = CloudStorageAccount
                    .Parse(configuration.StorageConnectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference("transformation");

                await _container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null);
            }

            cancellationToken.ThrowIfCancellationRequested();

            while (true)
            {
                var jobBlobs = _container
                    .ListBlobs("jobs/")
                    .Where(blob => !blob.StorageUri.PrimaryUri.ToString().Contains("$$$ORCHARD$$$.$$$"))
                    .Cast<CloudBlockBlob>();

                foreach (var jobBlob in jobBlobs)
                {
                    using (var stream = await jobBlob.OpenReadAsync(cancellationToken))
                    using (var streamReader = new StreamReader(stream))
                    {
                        var job = _jsonConverter.Deserialize<TransformationJob>(await streamReader.ReadToEndAsync());
                        System.Diagnostics.Debugger.Break();
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            //var hardwareImplementation = await _hastlayer.GenerateHardware();
        }

        public void Dispose()
        {
            if (_hastlayer == null) return;

            _hastlayer.Dispose();
            _hastlayer = null;
        }
    }
}
