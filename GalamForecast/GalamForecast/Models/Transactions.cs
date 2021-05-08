using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Transactions
    {
        public Transactions(DateTime creationDate, int year, int quarter, string status, double qty, string itemNumber,
            string itemDescription, string productFamilyName, int customerNumber, string customerName, string endCustomer,
            string meltingClassic, string personFullName, string countryName, string market, string createdBy, DateTime lastUpdateDate, string lastUpdatedBy)
        {
            CreationDate = creationDate;
            Year = year;
            Quarter = quarter;
            Status = status;
            Qty = qty;
            ItemNumber = itemNumber;
            ItemDescription = itemDescription;
            ProductFamilyName = productFamilyName;
            CustomerNumber = customerNumber;
            CustomerName = customerName;
            EndCustomer = endCustomer;
            MeltingClassic = meltingClassic;
            PersonFullName = personFullName;
            CountryName = countryName;
            Market = market;
            CreatedBy = createdBy;
            LastUpdateDate = lastUpdateDate;
            LastUpdatedBy = lastUpdatedBy;
        }

        public Transactions() { }

        public DateTime CreationDate { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public string Status { get; set; }
        public double Qty { get; set; }
        public string ItemNumber { get; set; }
        public string ItemDescription { get; set; }
        public string ProductFamilyName { get; set; }
        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string EndCustomer { get; set; }
        public string MeltingClassic { get; set; }
        public string PersonFullName { get; set; }
        public string CountryName { get; set; }
        public string Market { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string LastUpdatedBy { get; set; }

        public static List<Transactions> GetTransactions(string from = null, string to = null, string year = null, string quarter = null,
            string customerNumber = null, string shipToName = null, string itemNumber = null, string productFamilyId = null, string market = null, string countryId = null)
        {
            return DBServices.GetTransactions(from, to, year, quarter, customerNumber, shipToName, itemNumber, productFamilyId, market, countryId);
        }
    }
}