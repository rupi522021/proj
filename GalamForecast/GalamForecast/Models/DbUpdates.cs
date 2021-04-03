using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class DbUpdates
    {
        public DbUpdates(string functionName, string report)
        {
            FunctionName = functionName;
            Report = report;
        }

        public DbUpdates() { }

        public DbUpdates(string functionName, string report, int numOfErrors, int numOfGoods)
        {
            FunctionName = functionName;
            Report = report;
            NumOfErrors = numOfErrors;
            this.numOfGoods = numOfGoods;
        }

        public DbUpdates(int numOfErrors, int numOfGoods)
        {
            NumOfErrors = numOfErrors;
            this.numOfGoods = numOfGoods;
        }

        public string FunctionName { get; set; }
        public string Report { get; set; }
        public int NumOfErrors { get; set; }
        public int numOfGoods { get; set; }

        public static List<DbUpdates> UpdateTables()
        {
            List<DbUpdates> result = new List<DbUpdates>
            {
                Periods.AddNewPeriods(),
                Countries.UpdateCountries(),
                Person.UpdatePeople(),
                SaleMenagers.UpdateSaleMenagers(),
                Customers.UpdateCustomers(),
                Items.UpdateItems(),
                SaleRows.UpdateSaleRows(),
                Weeks.UpdateWeeks()
            };

            return result;
        }
    }
}