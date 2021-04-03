using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class ForecastRows
    {
        public static Dictionary<string, Lockes> LockedForecastRows;
        public static Dictionary<int, Lockes> LockedCustomerNumbers;
        static ForecastRows()
        {
            LockedForecastRows = new Dictionary<string, Lockes>();
            LockedCustomerNumbers = new Dictionary<int, Lockes>();
        }

        public ForecastRows(string itemNumber, int year, int customerNumber, string shipToName, string meltingClassic,
            double q1, double q2, double q3, double q4, string countryName, string market, string itemDescription, string productFamilyName, string customerName, string personFullName)
        {
            ItemNumber = itemNumber;
            Year = year;
            CustomerNumber = customerNumber;
            ShipToName = shipToName;
            MeltingClassic = meltingClassic;
            Q1 = q1;
            Q2 = q2;
            Q3 = q3;
            Q4 = q4;
            CountryName = countryName;
            Market = market;
            ItemDescription = itemDescription;
            ProductFamilyName = productFamilyName;
            CustomerName = customerName;
            PersonFullName = personFullName;
        }

        public ForecastRows() { }

        public string ItemNumber { get; set; }
        public int Year { get; set; }
        public int CustomerNumber { get; set; }
        public string ShipToName { get; set; }
        public string MeltingClassic { get; set; }
        public double Q1 { get; set; }
        public double Q2 { get; set; }
        public double Q3 { get; set; }
        public double Q4 { get; set; }
        public string CountryName { get; set; }
        public string Market { get; set; }
        public string ItemDescription { get; set; }
        public string ProductFamilyName { get; set; }
        public string CustomerName { get; set; }
        public string PersonFullName { get; set; }
        public double Total => Math.Round(Q1 + Q2 + Q3 + Q4, 1);
        public string StringKey => $"{ItemNumber}|{Year}|{CustomerNumber}|{ShipToName}";
        public string LockedToUser { get; set; }
        public List<int> QuartersToUpdate { get; set; }

        public double QValue(int q)
        {
            if (q == 1) return Q1;
            if (q == 2) return Q2;
            if (q == 3) return Q3;
            return Q4;
        }

        public static List<ForecastRows> GetForecast(int year, string fDate) { return DBServices.GetForecast(year == 0 ? Weeks.CurrentYear() : year, fDate); }

        public static List<int> GetForecastYears() { return DBServices.GetForecastYears(); }

        public static List<int> GetForecastUpdateYears() { return new List<int> { Weeks.CurrentYear(), Weeks.CurrentYear() + 1 }; }

        public static List<ForecastRows> GetForecastToUpdate(int year, Users user)
        {
            List<ForecastRows> result = DBServices.GetForecast(year == 0 ? Weeks.CurrentYear() : year, null, user);
            for (int i = 0; i < result.Count; i++)
            {
                if (LockedForecastRows.ContainsKey(result[i].StringKey))
                {
                    if (LockedForecastRows[result[i].StringKey].LockedToUser == user.UserName) LockedForecastRows[result[i].StringKey].LockedDateTime = DateTime.Now;
                    result[i].LockedToUser = LockedForecastRows[result[i].StringKey].LockedToUser;
                }
                else
                {
                    result[i].LockedToUser = user.UserName;
                    LockedForecastRows[result[i].StringKey] = new Lockes(user.UserName, DateTime.Now);
                }
            }
            return result;
        }

        public static List<int> GetLockedQuarters(int year)
        {
            int tmpYear = year == 0 ? Weeks.CurrentYear() : year;
            if (tmpYear < Weeks.CurrentYear()) return new List<int> { 1, 2, 3, 4 };
            if (tmpYear > Weeks.CurrentYear()) return new List<int>();
            List<int> results = new List<int>();
            for (int i = 1; i <= 4; i++) if (i < Weeks.CurrentQuarter()) results.Add(i);
            return results;
        }

        public static bool ClearLockes(Users user)
        {
            List<string> ForecastKeysToRemove = new List<string>();
            foreach (KeyValuePair<string, Lockes> kvp in LockedForecastRows) if (kvp.Value.LockedToUser == user.UserName) ForecastKeysToRemove.Add(kvp.Key);
            foreach (string key in ForecastKeysToRemove) LockedForecastRows.Remove(key);
            ClearCustomerLockes(user);
            return true;
        }

        public static bool ClearCustomerLockes(Users user)
        {
            List<int> CustomerNumbersToRemove = new List<int>();
            foreach (KeyValuePair<int, Lockes> kvp in LockedCustomerNumbers) if (kvp.Value.LockedToUser == user.UserName) CustomerNumbersToRemove.Add(kvp.Key);
            foreach (int key in CustomerNumbersToRemove) LockedCustomerNumbers.Remove(key);
            return true;
        }

        public static bool UpdateForecast(Users user, List<ForecastRows> forecastRowFromClient)
        {
            List<ForecastRows> forecastToUpdate = new List<ForecastRows>();
            foreach (ForecastRows row in forecastRowFromClient)
                if (LockedForecastRows.ContainsKey(row.StringKey) && !(row.QuartersToUpdate is null))
                    if (LockedForecastRows[row.StringKey].LockedToUser == row.LockedToUser && LockedForecastRows[row.StringKey].LockedToUser == user.UserName && row.QuartersToUpdate.Count > 0)
                        if (row.Q1 > 0 && row.Q2 > 0 && row.Q3 > 0 && row.Q4 > 0) forecastToUpdate.Add(row);
            if (forecastToUpdate.Count == 0) return false;
            return DBServices.UpdateForecast(user, forecastToUpdate);
        }

        public static ForecastRows AddNewForecastRow(Users user, ForecastRows row)
        {
            Customers customer = DBServices.GetCustomer(row.CustomerNumber);
            if (!user.Permissions.Contains(1) && !(user.IsSaleMenager && user.PersonId == customer.SaleMenagerId) && !(user.Permissions.Contains(2) && customer.IsLocal)) throw new Exception("Forbidden");
            ForecastRows result = DBServices.AddNewForecastRow(user, row, customer);
            result.LockedToUser = user.UserName;
            LockedForecastRows[result.StringKey] = new Lockes(user.UserName, DateTime.Now);
            return result;
        }
    }
}