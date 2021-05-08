using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class SaleRows
    {
        public SaleRows(int year, int quarter, string itemNumber, int customerNumber, double qty)
        {
            Year = year;
            Quarter = quarter;
            ItemNumber = itemNumber;
            CustomerNumber = customerNumber;
            Qty = qty;
        }

        public SaleRows() { }

        public int Year { get; set; }
        public int Quarter { get; set; }
        public string ItemNumber { get; set; }
        public int CustomerNumber { get; set; }
        public double Qty { get; set; }

        public bool equalKey(SaleRows b)
        {
            return Year == b.Year && Quarter == b.Quarter && ItemNumber == b.ItemNumber && Qty == b.Qty;
        }

        public static DbUpdates UpdateSaleRows()
        {
            try
            {
                int a = DBServices.GetMaxSalePeriod() + 1;
                int b = Periods.PeriodNumber(DateTime.Today) - 1;
                List<SaleRows> ToAdd = ErpDBServices.GetErpSaleRows(DBServices.GetMaxSalePeriod() + 1, Periods.PeriodNumber(DateTime.Today) - 1);
                if (ToAdd.Count == 0) return new DbUpdates("UpdateSales", "Done");
                DbUpdates result = DBServices.AddUpdateSaleRows(ToAdd);
                result.FunctionName = "UpdateSales";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateSales", "Error"); }
        }
    }
}