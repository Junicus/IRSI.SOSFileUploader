using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Configuration;
using IRSI.SOSFileUploader.Models.Common;
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
            response.EnsureSuccessStatusCode();
            var storeJson = await response.Content.ReadAsStringAsync();
            var store = JsonConvert.DeserializeObject<Store>(storeJson);
            return store;
        }

        public async Task<HttpResponseMessage> PostSOSFile(Guid storeId, byte[] fileData, string fileName)
        {
            var requestContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/sos");

            requestContent.Add(fileContent, fileName, fileName);

            return await PostAsync($"api/sos/stores/{storeId}/uploadSOS", requestContent);
        }
    }
}
