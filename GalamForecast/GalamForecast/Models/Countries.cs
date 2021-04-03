using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Countries
    {
        public Countries(string countryId, string countryName, string countryMarket)
        {
            CountryId = countryId;
            CountryName = countryName;
            CountryMarket = countryMarket;
        }

        public Countries(string countryId)
        {
            CountryId = countryId;
        }

        public Countries() { }

        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryMarket { get; set; }

        public static DbUpdates UpdateCountries()
        {
            try
            {
                List<Countries> ErpCountries = ErpDBServices.GetErpCountries();
                Dictionary<string, Countries> DbCountries = DBServices.GetCountries();
                List<Countries> countriesToAdd = new List<Countries>();
                List<Countries> countriesToUpdate = new List<Countries>();
                for (int i = 0; i < ErpCountries.Count; i++)
                {
                    if (!DbCountries.ContainsKey(ErpCountries[i].CountryId)) countriesToAdd.Add(ErpCountries[i]);
                    else
                    {
                        Countries tmp = DbCountries[ErpCountries[i].CountryId];
                        if (tmp.CountryName != ErpCountries[i].CountryName || tmp.CountryMarket != ErpCountries[i].CountryMarket) countriesToUpdate.Add(ErpCountries[i]);
                    }
                }
                DbUpdates result = DBServices.AddUpdateCountries(countriesToAdd, countriesToUpdate);
                result.FunctionName = "UpdateCountries";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateCountries", "Error"); }
        }

        public static List<Countries> GetCountries() { return DBServices.GetCountries().Values.ToList(); }
    }
}