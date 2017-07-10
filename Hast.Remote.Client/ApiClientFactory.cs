using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RestEase;

namespace Hast.Remote.Client
{
    public static class ApiClientFactory
    {
        public static IHastlayerApi CreateApiClient(HastlayerRemoteClientConfiguration configuration)
        {
            var api = RestClient.For<IHastlayerApi>(configuration.EndpointBaseUri);

            api.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(configuration.AppId + ":" + configuration.AppSecret)));

            return api;
        }


        public interface IHastlayerApi
        {
            [Header("Authorization")]
            AuthenticationHeaderValue Authorization { get; set; }

            //[Get("SupportedDevices")]
            //Task<IEnumerable<DeviceManifest>> GetSupportedDevices();
        }
    }
}
