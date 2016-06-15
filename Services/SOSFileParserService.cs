using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Models.Common;
using IRSI.SOSFileUploader.Models.SOS;
using Serilog;

namespace IRSI.SOSFileUploader.Services
{
    public class SOSFileParserService : ISOSFileParserService
    {
        private readonly ISOSLineParserService _sosLineParser;
        private readonly ILogger _log;

        public SOSFileParserService(ISOSLineParserService sosLineParser)
        {
            _log = Log.ForContext<SOSFileParserService>();
            _sosLineParser = sosLineParser;
        }

        public async Task<IList<SOSItem>> ParseAsync(string path, Store store)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!File.Exists(path)) throw new FileNotFoundException($"Cannot locate {path}", path);

            try
            {
                var lines = new List<string>();
                using (var reader = File.OpenRead(path))
                {
                    using (var streamReader = new StreamReader(reader))
                    {
                        var data = await streamReader.ReadToEndAsync();
                        lines.AddRange(data.Split('\n'));
                    }
                }

                var fileName = Path.GetFileNameWithoutExtension(path);
                var businessDateString = fileName.Substring(fileName.Length - 8, 8);
                var businessDate = DateTime.MinValue;

                var result = new List<SOSItem>();
                if (DateTime.TryParseExact(businessDateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out businessDate))
                {
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            var sosItem = _sosLineParser.Parse(line, store, businessDate.AddDays(-1));
                            result.Add(sosItem);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error parsing file: {file}", path);
                throw ex;
            }
        }
    }
}
