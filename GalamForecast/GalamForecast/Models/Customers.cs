using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Customers
    {
        public Customers(int number, string name, bool isLocal, int saleMenagerId, Countries country)
        {
            Number = number;
            Name = name;
            IsLocal = isLocal;
            SaleMenagerId = saleMenagerId;
            Country = country;
        }

        public Customers(int number, string name, bool isLocal, int saleMenagerId, string country)
        {
            Number = number;
            Name = name;
            IsLocal = isLocal;
            SaleMenagerId = saleMenagerId;
            Country = new Countries(country);
        }

        public Customers(int number, string name)
        {
            Number = number;
            Name = name;
        }

        public Customers() { }

        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsLocal { get; set; }
        public int SaleMenagerId { get; set; }
        public Countries Country { get; set; }

        public static DbUpdates UpdateCustomers()
        {
            try
            {
                List<Customers> ErpData = ErpDBServices.GetErpCustomers();
                Dictionary<int, Customers> DbData = DBServices.GetCustomers();
                List<Customers> ToAdd = new List<Customers>();
                List<Customers> ToUpdate = new List<Customers>();
                for (int i = 0; i < ErpData.Count; i++)
                {
                    if (!DbData.ContainsKey(ErpData[i].Number)) ToAdd.Add(ErpData[i]);
                    else
                    {
                        Customers tmp = DbData[ErpData[i].Number];
                        if (tmp.Name != ErpData[i].Name || tmp.IsLocal != ErpData[i].IsLocal || tmp.SaleMenagerId != ErpData[i].SaleMenagerId || tmp.Country.CountryId != ErpData[i].Country.CountryId) ToUpdate.Add(ErpData[i]);
                    }
                }
                DbUpdates result = DBServices.AddUpdateCustomers(ToAdd, ToUpdate);
                result.FunctionName = "UpdateCustomers";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateCustomers", "Error"); }
        }
    }
}