using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class ShipTos
    {

        public ShipTos() { }

        public ShipTos(int customerNumber, string customerName, string shipToName, string countryId, string countryName, bool active, bool isEndCustomer)
        {
            CustomerNumber = customerNumber;
            CustomerName = customerName;
            ShipToName = shipToName;
            CountryId = countryId;
            CountryName = countryName;
            Active = active;
            IsEndCustomer = isEndCustomer;
        }

        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string ShipToName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public bool Active { get; set; }
        public bool IsEndCustomer { get; set; }

        public static List<ShipTos> GetShipTos() { return DBServices.GetShipTos(); }

        public static bool UpdateShipToActive(ShipTos ShipTo) { return DBServices.UpdateShipToActive(ShipTo); }

        public static ShipTos AddShipTo(ShipTos ShipTo) { return DBServices.AddShipTo(ShipTo); }
    }
}