using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Configuration;
using IRSI.SOSFileUploader.Models.Common;
using IRSI.SOSFileUploader.Models.SOS;
using Newtonsoft.Json;
using Serilog;

namespace IRSI.SOSFileUploader.ApiClients
{
    public class SOSApiClient : HttpClient
    {
        private TokenClient _tokenClient;
        private readonly ILogger _log;

        public SOSApiClient(SOSApiClientOptions options, TokenClient tokenClient)
        {
            _log = Log.ForContext<SOSApiClient>();
            _tokenClient = tokenClient;
            BaseAddress = new Uri(options.ApiUrl);
            var token = _tokenClient.GetBearerAccessTokenAsync(options.ClientId, options.ClientSecret).Result;
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<Store> GetStoreAsync(Guid storeId)
        {
            _log.Information("Getting Store: {storeId}", storeId);
            try
            {
                var response = await GetAsync($"api/sos/stores/{storeId}");
                if (response.IsSuccessStatusCode)
                {
                    var storeJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(storeJson))
                    {
                        var store = JsonConvert.DeserializeObject<Store>(storeJson);
                        return store;
                    }
                    else
                    {
                        _log.Error("API call successful but record not found");
                        return null;
                    }
                }
                else
                {
                    _log.Error(await response.Content.ReadAsStringAsync());
                    return null;
                }
            } catch(Exception ex)
            {
                _log.Error(ex, "Error retreiving store");
                return null;
            }
        }

        public async Task<HttpResponseMessage> PostSOSFile(SOSItemsPost sosItemsPost)
        {
            _log.Information("Posting SOSItems");
            var sosJson = JsonConvert.SerializeObject(sosItemsPost);
            return await PostAsync($"api/sos/stores/{sosItemsPost.StoreId}/uploadSOS", new StringContent(sosJson, Encoding.UTF8, "application/json"));
        }
    }
}
