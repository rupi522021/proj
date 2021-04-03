using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Items
    {
        public Items(string number, string description, string productFamilyId)
        {
            Number = number;
            Description = description;
            ProductFamilyId = productFamilyId;
        }

        public Items() { }

        public string Number { get; set; }
        public string Description { get; set; }
        public string ProductFamilyId { get; set; }

        public static DbUpdates UpdateItems()
        {
            try
            {
                List<Items> ErpData = ErpDBServices.GetErpItems();
                Dictionary<string, Items> DbData = DBServices.GetItems();
                List<Items> ToAdd = new List<Items>();
                List<Items> ToUpdate = new List<Items>();
                for (int i = 0; i < ErpData.Count; i++)
                {
                    if (!DbData.ContainsKey(ErpData[i].Number)) ToAdd.Add(ErpData[i]);
                    else
                    {
                        Items tmp = DbData[ErpData[i].Number];
                        if (tmp.Description != ErpData[i].Description || tmp.ProductFamilyId != ErpData[i].ProductFamilyId) ToUpdate.Add(ErpData[i]);
                    }
                }
                DbUpdates result = DBServices.AddUpdateItems(ToAdd, ToUpdate);
                result.FunctionName = "UpdateItems";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateItems", "Error"); }
        }

        public static List<Items> GetAllItems() { return DBServices.GetItems().Values.ToList(); }
    }
}