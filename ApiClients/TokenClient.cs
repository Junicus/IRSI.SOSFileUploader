using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;

namespace IRSI.SOSFileUploader.ApiClients
{
    public class TokenClient : HttpClient
    {
        private readonly ILogger _log;

        public TokenClient(TokenClientOptions options)
        {
            _log = Log.ForContext<TokenClient>();
            BaseAddress = new Uri(options.TokenUrl);
        }

        public async Task<string> GetBearerAccessTokenAsync(string clientId, string clientSecret)
        {
            _log.Information("Getting AccessToken...");
            DefaultRequestHeaders.Accept.Clear();
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials(clientId, clientSecret));
            try
            {
                var response = await PostAsync(string.Empty,
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "scope", "sos_api" }
                    }));

                if (response.IsSuccessStatusCode)
                {
                    _log.Information("AccessToken retreived successfuly");
                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());

                    var token = json["access_token"].ToString();
                    return token;
                }
                else
                {
                    _log.Debug("AccessToken retreive failed");
                    _log.Debug(await response.Content.ReadAsStringAsync());
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error adquiring token");
                return string.Empty;
            }
        }

        private string EncodeCredentials(string username, string password)
        {
            var credential = $"{username}:{password}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(credential));
        }
    }
}
