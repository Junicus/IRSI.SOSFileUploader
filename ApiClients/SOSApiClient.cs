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

namespace IRSI.SOSFileUploader.ApiClients
{
    public class SOSApiClient : HttpClient
    {
        private TokenClient _tokenClient;

        public SOSApiClient(SOSApiClientOptions options, TokenClient tokenClient)
        {
            _tokenClient = tokenClient;
            BaseAddress = new Uri(options.ApiUrl);
            var token = _tokenClient.GetBearerAccessTokenAsync(options.ClientId, options.ClientSecret).Result;
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<Store> GetStoreAsync(Guid storeId)
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
                    //log error but return null;
                    string errContent = "API call successful but record not found";
                    return null;
                }
            }
            else
            {
                //Log error but ruturn null
                var errContent = await response.Content.ReadAsStringAsync();
                return null;
            }
        }

        public async Task<HttpResponseMessage> PostSOSFile(SOSItemsPost sosItemsPost)
        {
            var sosJson = JsonConvert.SerializeObject(sosItemsPost);
            return await PostAsync($"api/sos/stores/{sosItemsPost.StoreId}/uploadSOS", new StringContent(sosJson, Encoding.UTF8, "application/json"));
        }
    }
}
