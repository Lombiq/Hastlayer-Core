using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RestEase;

namespace Hast.Remote.Client
{
    internal static class ApiClientFactory
    {
        public static IHastlayerApi CreateApiClient(Uri baseUri)
        {
            var api = RestClient.For<IHastlayerApi>(baseUri);

            var value = Convert.ToBase64String(Encoding.ASCII.GetBytes("username:password1234"));
            api.Authorization = new AuthenticationHeaderValue("Basic", value);

            return api;
        }


        public interface IHastlayerApi
        {
            [Header("Authorization")]
            AuthenticationHeaderValue Authorization { get; set; }
        }
    }
}
