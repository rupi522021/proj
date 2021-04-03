using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class SaleMenagers
    {
        public SaleMenagers(int id)
        {
            Id = id;
        }

        public SaleMenagers(int id, bool active)
        {
            Id = id;
            Active = active;
        }

        public SaleMenagers() { }

        public int Id { get; set; }
        public bool Active { get; set; }

        public static DbUpdates UpdateSaleMenagers()
        {
            try
            {
                List<int> ErpData = ErpDBServices.GetErpSaleMenagers();
                Dictionary<int, SaleMenagers> DbData = DBServices.GetSaleMenagers();
                List<SaleMenagers> ToAdd = new List<SaleMenagers>();
                List<SaleMenagers> ToUpdate = new List<SaleMenagers>();
                for (int i = 0; i < ErpData.Count; i++)
                {
                    if (!DbData.ContainsKey(ErpData[i])) ToAdd.Add(new SaleMenagers(ErpData[i], true));
                }
                foreach (KeyValuePair<int, SaleMenagers> kvp in DbData)
                {
                    if (!ErpData.Contains(kvp.Key) && kvp.Value.Active) ToUpdate.Add(new SaleMenagers(kvp.Key, false));
                }
                DbUpdates result = DBServices.AddUpdateSaleMenagers(ToAdd, ToUpdate);
                result.FunctionName = "UpdateSaleMenagers";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateSaleMenagers", "Error"); }
        }
    }
}