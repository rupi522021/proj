using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class ApprovalRows
    {
        public ApprovalRows(DateTime creationDate, string itemNumber, int year, int quarter, int customerNumber,
            string shipToName, double qty, string countryName, string market,
            string itemDescription, string productFamilyName, string customerName, string personFullName, int id)
        {
            CreationDate = creationDate;
            ItemNumber = itemNumber;
            Year = year;
            Quarter = quarter;
            CustomerNumber = customerNumber;
            ShipToName = shipToName;
            Qty = qty;
            CountryName = countryName;
            Market = market;
            ItemDescription = itemDescription;
            ProductFamilyName = productFamilyName;
            CustomerName = customerName;
            PersonFullName = personFullName;
            Id = id;
        }

        public ApprovalRows() { }

        public DateTime CreationDate { get; set; }
        public string ItemNumber { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int CustomerNumber { get; set; }
        public string ShipToName { get; set; }
        public double Qty { get; set; }
        public string CountryName { get; set; }
        public string Market { get; set; }
        public string ItemDescription { get; set; }
        public string ProductFamilyName { get; set; }
        public string CustomerName { get; set; }
        public string PersonFullName { get; set; }
        public int Id { get; set; }

        public static int GetNumOfApprovals(Users user) { return DBServices.GetNumOfApprovals(user); }

        public static List<ApprovalRows> GetApprovals(Users user) { return DBServices.GetApprovals(user); }

        public static List<ApprovalRows> PostApprovals(Users user,int id, string action)
        {
            if (!DBServices.PostApprovals(user, id, action)) throw new Exception("BadRequest");
            return DBServices.GetApprovals(user);
        }
    }
}