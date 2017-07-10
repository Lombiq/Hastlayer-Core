﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hast.Remote.Bridge.Models;
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

            [Post("TransformationRequests")]
            Task<TransformationTicket> RequestTransformation([Body] TransformationRequest transformationRequest);

            [Get("TransformationResults"), AllowAnyStatusCode]
            Task<Response<TransformationResult>> GetTransformationResult([Query] string transformationToken);
        }
    }
}
