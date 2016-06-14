using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.ApiClients;
using IRSI.SOSFileUploader.Configuration;
using IRSI.SOSFileUploader.Models.Common;
using IRSI.SOSFileUploader.Models.SOS;
using IRSI.SOSFileUploader.Services;

namespace IRSI.SOSFileUploader
{
    public class SOSFileUploader
    {
        private SOSApiClient _sosApiClient;
        private Store _store;
        private string _qsrPath;
        private readonly IFileHistoryService _fileHistoryService;
        private readonly ISOSFileParserService _sosFileParser;

        public SOSFileUploader(SOSFileUploaderOptions options,
            SOSApiClient sosApiClient,
            IFileHistoryService fileHistoryService,
            ISOSFileParserService sosFileParser)
        {
            _qsrPath = options.QsrSOSPath;
            _fileHistoryService = fileHistoryService;
            _sosApiClient = sosApiClient;
            _store = _sosApiClient.GetStoreAsync(options.StoreId).Result;
            _sosFileParser = sosFileParser;
        }

        public async Task RunAsync()
        {
            if (_store != null)
            {
                await _fileHistoryService.LoadAsync();
                var files = Directory.GetFiles(_qsrPath, "*.kst");
                foreach (var file in files)
                {
                    if (!file.Contains("ServTime.kst"))
                    {
                        if (_fileHistoryService.IsFileNew(file))
                        {
                            var sosItems = await _sosFileParser.ParseAsync(file, _store);
                            var sosItemsPost = new SOSItemsPost()
                            {
                                StoreId = _store.Id,
                                Filename = file,
                                BusinessDate = sosItems.First().DateOfBusiness,
                                SOSItems = sosItems
                            };
                            var response = await _sosApiClient.PostSOSFile(sosItemsPost);
                            if (response.IsSuccessStatusCode)
                            {
                                _fileHistoryService.AddFile(file);
                                await _fileHistoryService.SaveAsync();
                            }
                        }
                    }
                }
            }
            else
            {
                //log error store not found
                return;
            }
        }
    }
}
