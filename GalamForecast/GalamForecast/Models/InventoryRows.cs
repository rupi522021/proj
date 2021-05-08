using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class InventoryRows
    {
        public static List<int> GetInventoryYears() { return new List<int> { Weeks.CurrentYear() - 1, Weeks.CurrentYear(), Weeks.CurrentYear() + 1 }; }

        public static bool PostInventoryParams(Dictionary<string, double> distD, List<Periods> periods) { return DBServices.PostInventoryParams(distD, periods); }
    }
}