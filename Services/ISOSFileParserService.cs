using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Models.Common;
using IRSI.SOSFileUploader.Models.SOS;

namespace IRSI.SOSFileUploader.Services
{
    public interface ISOSFileParserService
    {
        Task<IList<SOSItem>> ParseAsync(string path, Store store);
    }
}
