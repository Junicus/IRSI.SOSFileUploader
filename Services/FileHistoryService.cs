using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Configuration;
using Newtonsoft.Json;

namespace IRSI.SOSFileUploader.Services
{
    public class FileHistoryService : IFileHistoryService
    {
        private string _historyFilePath;
        private List<string> _files;


        public FileHistoryService(FileHistoryServiceOptions options)
        {
            _historyFilePath = options.HistoryFilePath;
            _files = new List<string>();
        }
        public void AddFile(string filename)
        {
            if (_files.Contains(filename)) return;

            _files.Add(filename);
        }

        public bool IsFileNew(string filename)
        {
            if (_files.Contains(filename))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task LoadAsync()
        {
            using (FileStream fs = new FileStream(_historyFilePath, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string json = await sr.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(json))
                        _files = JsonConvert.DeserializeObject<List<string>>(json);
                }
            }
        }

        public async Task SaveAsync()
        {
            using (FileStream fs = new FileStream(_historyFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(JsonConvert.SerializeObject(_files));
                }
            }
        }
    }
}
