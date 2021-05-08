using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class ForecastSum
    {
        public ForecastSum(int year, int quarter, double fructoseS, double fructoseN, double fructoseD, double fructoseE, double fructoseOF, double fructoseL)
        {
            Year = year;
            Quarter = quarter;
            FructoseS = Math.Round(fructoseS, 0);
            FructoseN = Math.Round(fructoseN, 0);
            FructoseD = Math.Round(fructoseD, 0);
            FructoseE = Math.Round(fructoseE, 0);
            FructoseOF = Math.Round(fructoseOF, 0);
            FructoseL = Math.Round(fructoseL, 0);
        }

        public ForecastSum() { }

        public int Year { get; set; }
        public int Quarter { get; set; }
        public double FructoseS { get; set; }
        public double FructoseN { get; set; }
        public double FructoseD { get; set; }
        public double FructoseE { get; set; }
        public double FructoseOF { get; set; }
        public double FructoseL { get; set; }
        public double Total => Math.Round(FructoseS + FructoseN + FructoseD + FructoseE + FructoseOF + FructoseL, 1);
        public int FructoseSPercent => (int)(100 * FructoseS / Total);
        public int FructoseNPercent => (int)(100 * FructoseN / Total);
        public int FructoseDPercent => (int)(100 * FructoseD / Total);
        public int FructoseEPercent => (int)(100 * FructoseE / Total);
        public int FructoseOFPercent => (int)(100 * FructoseOF / Total);
        public int FructoseLPercent => (int)(100 * FructoseL / Total);
        public int PeriodNumber => Periods.PeriodNumber(Year, Quarter);


        public static Dictionary<int, ForecastSum> GetForecastSum(int yearFrom, int yearTo) { return DBServices.GetForecastSum(yearFrom, yearTo); }

        public static List<ForecastSum> GetForecastSum() { return DBServices.GetForecastSum(Weeks.CurrentYear(), Weeks.CurrentYear()).Values.ToList(); }
    }
}