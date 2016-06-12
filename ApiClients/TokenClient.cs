using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Configuration;
using Newtonsoft.Json.Linq;

namespace IRSI.SOSFileUploader.ApiClients
{
    public class TokenClient : HttpClient
    {
        public TokenClient(TokenClientOptions options)
        {
            BaseAddress = new Uri(options.TokenUrl);
        }

        public async Task<string> GetBearerAccessTokenAsync(string clientId, string clientSecret)
        {
            DefaultRequestHeaders.Accept.Clear();
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials(clientId, clientSecret));

            var response = await PostAsync(string.Empty,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "scope", "sos_api" }
                }));

            response.EnsureSuccessStatusCode();
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            var token = json["access_token"].ToString();
            return token;
        }

        private string EncodeCredentials(string username, string password)
        {
            var credential = $"{username}:{password}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(credential));
        }
    }
}
