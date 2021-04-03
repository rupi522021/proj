using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Weeks
    {
        public Weeks(int yearP, int quarterP, int weekNumber, double actualProduction, double actualGreens, double actualSales,
            double actualInventory, double actualInventoryS, double actualInventoryN, double actualInventoryD, double actualInventoryE, double actualInventoryOF, double actualInventoryExternal)
        {
            YearP = yearP;
            QuarterP = quarterP;
            WeekNumber = weekNumber;
            ActualProduction = actualProduction;
            ActualGreens = actualGreens;
            ActualSales = actualSales;
            ActualInventory = actualInventory;
            ActualInventoryS = actualInventoryS;
            ActualInventoryN = actualInventoryN;
            ActualInventoryD = actualInventoryD;
            ActualInventoryE = actualInventoryE;
            ActualInventoryOF = actualInventoryOF;
            ActualInventoryExternal = actualInventoryExternal;
        }

        public Weeks() { }

        public int YearP { get; set; }
        public int QuarterP { get; set; }
        public int WeekNumber { get; set; }
        public double ActualProduction { get; set; }
        public double ActualGreens { get; set; }
        public double ActualSales { get; set; }
        public double ActualInventory { get; set; }
        public double ActualInventoryS { get; set; }
        public double ActualInventoryN { get; set; }
        public double ActualInventoryD { get; set; }
        public double ActualInventoryE { get; set; }
        public double ActualInventoryOF { get; set; }
        public double ActualInventoryExternal { get; set; }

        public static int NumOfWeeksInYear(int year)
        {
            return (FirstYearDay(year + 1).Date - FirstYearDay(year).Date).Days / 7;
        }

        public static DateTime FirstYearDay(int year)
        {
            DateTime tmp = new DateTime(year, 1, 1);
            int y11 = (int)tmp.DayOfWeek;
            if (y11 == 0) y11 = 7;
            DateTime result;
            if (y11 <= 4) result = new DateTime(year - 1, 12, 33 - y11);
            else result = new DateTime(year, 1, 9 - y11);
            return result;
        }

        public static int MaxAbsWeekNumber()
        {
            int tmp = DBServices.GetMaxAbsWeekNumber();
            if (tmp % 100 == 0) return tmp - 100 + NumOfWeeksInYear(tmp / 100 - 1);
            return tmp;
        }

        public static int DateYear(DateTime date)
        {
            if (date.Month == 1 && date < FirstYearDay(date.Year)) return date.Year - 1;
            if (date.Month == 12 && date >= FirstYearDay(date.Year + 1)) return date.Year + 1;
            else return date.Year;
        }

        public static int CurrentYear() { return DateYear(DateTime.Today); }

        public static int DateWeek(DateTime date) { return (date.Date - FirstYearDay(DateYear(date)).Date).Days / 7 + 1; }

        public static int CurrentWeek() { return DateWeek(DateTime.Today); }

        public static int CurrentQuarter()
        {
            int tmp = DateWeek(DateTime.Today);
            if (tmp <= 13) return 1;
            if (tmp <= 26) return 2;
            if (tmp <= 39) return 3;
            return 4;
        }

        public static int AbsWeekNumber(int year, int week) { return year * 100 + week; }

        public static int AbsWeekNumber(DateTime date) { return AbsWeekNumber(DateYear(date), DateWeek(date)); }

        public static int SubAbsWeekNumber(int absWeekNumber)
        {
            if (absWeekNumber % 100 == 1) return absWeekNumber - 101 + NumOfWeeksInYear(absWeekNumber / 100 - 1);
            return absWeekNumber - 1;
        }

        public static int AddAbsWeekNumber(int absWeekNumber)
        {
            if (absWeekNumber % 100 == NumOfWeeksInYear(absWeekNumber / 100)) return absWeekNumber + 101 - NumOfWeeksInYear(absWeekNumber / 100);
            return absWeekNumber + 1;
        }

        public static DbUpdates UpdateWeeks()
        {
            try
            {
                int minWeek = MaxAbsWeekNumber();
                int maxWeek = AbsWeekNumber(DateTime.Today);
                if (AddAbsWeekNumber(minWeek)>= maxWeek) return new DbUpdates("UpdateWeeks", "Done", 0, 0);
                List<Weeks> ErpData = ErpDBServices.GetErpWeeks(minWeek, maxWeek);

                DbUpdates result = DBServices.AddWeeks(ErpData);
                result.FunctionName = "UpdateWeeks";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateWeeks", "Error"); }
        }
    }
}