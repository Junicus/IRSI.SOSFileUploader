using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.ApiClients;
using IRSI.SOSFileUploader.Configuration;
using IRSI.SOSFileUploader.Models.Common;
using IRSI.SOSFileUploader.Services;

namespace IRSI.SOSFileUploader
{
    public class SOSFileUploader
    {
        private SOSApiClient _sosApiClient;
        private Store _store;
        private string _qsrPath;
        private readonly IFileHistoryService _fileHistoryService;
        
        public SOSFileUploader(SOSFileUploaderOptions options,
            SOSApiClient sosApiClient,
            IFileHistoryService fileHistoryService)
        {
            _qsrPath = options.QsrSOSPath;
            _fileHistoryService = fileHistoryService;
            _sosApiClient = sosApiClient;
            _store = _sosApiClient.GetStoreAsync(options.StoreId).Result;
        }

        public async Task RunAsync()
        {
            await _fileHistoryService.LoadAsync();
            var files = Directory.GetFiles(_qsrPath, "*.kst");
            foreach(var file in files)
            {
                if(!file.Contains("ServiceTime.kst"))
                {
                    if(_fileHistoryService.IsFileNew(file))
                    {
                        var response = await _sosApiClient.PostSOSFile(_store.Id, File.ReadAllBytes(file), file);
                        if (response.IsSuccessStatusCode)
                        {
                            _fileHistoryService.AddFile(file);
                            await _fileHistoryService.SaveAsync();
                        }
                    }
                }
            }
        }
    }
}
