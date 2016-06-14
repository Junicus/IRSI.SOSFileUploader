using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRSI.SOSFileUploader.Models.Common;
using IRSI.SOSFileUploader.Models.SOS;

namespace IRSI.SOSFileUploader.Services
{
    public class SOSLineParserService : ISOSLineParserService
    {
        public SOSItem Parse(string line, Store store, DateTime businessDate)
        {
            var result = new SOSItem();

            var splitParts = line.Split(',');

            result.DateOfBusiness = businessDate;
            try
            {
                result.StoreId = store.Id;
                result.RegionId = store.RegionId;
                result.ConceptId = store.ConceptId;
                result.TransactionNumber = long.Parse(splitParts[0]);
                result.Course = int.Parse(splitParts[1]);
                result.TerminalNumber = int.Parse(splitParts[2]);
                result.Destination = int.Parse(splitParts[3]);
                result.VirtualDisplayId = int.Parse(splitParts[4]);
                result.CurrentActivityLevel = int.Parse(splitParts[5]);
                result.DisplayGroupId = int.Parse(splitParts[6]);
                result.ItemId = int.Parse(splitParts[7]);
                result.Modifier1Id = int.Parse(splitParts[8]);
                result.Modifier2Id = int.Parse(splitParts[9]);
                result.Modifier3Id = int.Parse(splitParts[10]);
                result.OrderStartTime = new DateTime(int.Parse(splitParts[11]), int.Parse(splitParts[12]), int.Parse(splitParts[14]),
                    int.Parse(splitParts[15]), int.Parse(splitParts[16]), int.Parse(splitParts[17]));
                result.FirstStoreTime = int.Parse(splitParts[18]);
                result.LastTotalTime = int.Parse(splitParts[19]);
                result.LastRecallTime = int.Parse(splitParts[20]);
                result.OrderPaidTime = int.Parse(splitParts[21]);
                result.OrderFirstDisplayedTime = int.Parse(splitParts[22]);
                result.OrderParkTime = int.Parse(splitParts[23]);
                result.OrderLastBumpTime = int.Parse(splitParts[24]);
                return result;
            }
            catch (Exception ex)
            {
                //log.Error(string.Format("Error parsing line {0}", ex));
                throw new Exception(string.Format("Error parsing line {0}", line), ex);
            }
        }
    }
}
