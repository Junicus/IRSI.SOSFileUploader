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
using Serilog;

namespace IRSI.SOSFileUploader
{
    public class SOSFileUploader
    {
        private SOSApiClient _sosApiClient;
        private Store _store;
        private string _qsrPath;
        private readonly IFileHistoryService _fileHistoryService;
        private readonly ISOSFileParserService _sosFileParser;
        private readonly ILogger _log;

        public SOSFileUploader(SOSFileUploaderOptions options,
            SOSApiClient sosApiClient,
            IFileHistoryService fileHistoryService,
            ISOSFileParserService sosFileParser)
        {
            _log = Log.ForContext<SOSFileUploader>();
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
                _log.Information("Loading FileHistory...");
                await _fileHistoryService.LoadAsync();
                _log.Information("FileHistory loaded successfully");
                var files = Directory.GetFiles(_qsrPath, "*.kst");
                foreach (var file in files)
                {
                    if (!file.Contains("ServTime.kst"))
                    {
                        _log.Information("Processing {file}", file);
                        if (_fileHistoryService.IsFileNew(file))
                        {
                            _log.Information($"{file} is a new file");
                            var sosItems = await _sosFileParser.ParseAsync(file, _store);
                            _log.Information($"Parsed file with {sosItems.Count} records");
                            var sosItemsPost = new SOSItemsPost()
                            {
                                StoreId = _store.Id,
                                Filename = file,
                                BusinessDate = sosItems.First().DateOfBusiness,
                                SOSItems = sosItems
                            };
                            _log.Information("Posting to {StoreId} on {BusinessDate}", sosItemsPost.StoreId, sosItemsPost.BusinessDate);
                            var response = await _sosApiClient.PostSOSFile(sosItemsPost);
                            if (response.IsSuccessStatusCode)
                            {
                                _log.Information("Post successfull");
                                _log.Information("Saving FileHistory");
                                _fileHistoryService.AddFile(file);
                                await _fileHistoryService.SaveAsync();
                            } else
                            {
                                _log.Information("Post was unsuccessful");
                                var content = await response.Content.ReadAsStringAsync();
                                _log.Debug(content);
                            }
                        }
                    }
                }
            }
            else
            {
                _log.Error("Store not found");
                return;
            }
        }
    }
}
