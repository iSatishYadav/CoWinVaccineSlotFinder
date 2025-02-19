﻿using RestSharp;
using System;
using Microsoft.Extensions.Configuration;
using System.Net;
using CoWin.Auth;

namespace CoWin.Providers
{
    public class APIFacade
    {
        private readonly IConfiguration _configuration;
        public APIFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IRestResponse Get(string endpoint)
        {
            RestClient client = InitHttpClient(endpoint);

            IRestRequest request = new RestRequest(Method.GET);

            AddGenericHeaders(request);

            IRestResponse response = client.Execute(request);

            return response;
        }

        private void AddGenericHeaders(IRestRequest request)
        {
            request.AddHeader("accept", "application/json");
            request.AddHeader("Accept-Language", "en_US");

            if (Convert.ToBoolean(_configuration["CoWinAPI:ProtectedAPI:IsToBeUsed"]))
            {
                request.AddHeader("Origin", _configuration["CoWinAPI:SelfRegistrationPortal"]);
                request.AddHeader("Referer", _configuration["CoWinAPI:SelfRegistrationPortal"]);
                request.AddHeader("Authorization", $"Bearer {OTPAuthenticator.BEARER_TOKEN}");
            }
        }

        public IRestResponse Post(string endpoint, string body = null)
        {
            RestClient client = InitHttpClient(endpoint);

            IRestRequest request = new RestRequest(Method.POST);

            AddGenericHeaders(request);

            AddPostSpecificParameters(body, request);

            IRestResponse response = client.Execute(request);

            return response;
        }

        private static void AddPostSpecificParameters(string body, IRestRequest request)
        {
            request.AddHeader("Content-Type", "application/json");

            if(body != null)
            {
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }
            
        }

        private RestClient InitHttpClient(string endpoint)
        {
            var client = new RestClient(endpoint);
            client.Timeout = -1;
            client.UserAgent = _configuration["CoWinAPI:SpoofedUserAgentToBypassWAF"];

            if (Convert.ToBoolean(_configuration["Proxy:IsToBeUsed"]))
            {
                client.Proxy = new WebProxy
                {
                    Address = new Uri(_configuration["Proxy:Address"]),
                    UseDefaultCredentials = true
                };
            }

            return client;
        }
    }
}
