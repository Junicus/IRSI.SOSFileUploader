using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Configuration;

namespace IRSI.SOSFileUploader.Services
{
    public class FileHistoryService : IFileHistoryService
    {
        private string _historyFilePath;

        public FileHistoryService(FileHistoryServiceOptions options)
        {
            _historyFilePath = options.HistoryFilePath;
        }
        public void AddFile(string filename)
        {
            throw new NotImplementedException();
        }

        public bool IsFileNew(string filename)
        {
            throw new NotImplementedException();
        }

        public async Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SaveAsync()
        {
            throw new NotImplementedException();
        }
    }
}
