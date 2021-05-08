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

        public Weeks(int yearP, int quarterP, int weekNumber, double actualProduction, double actualGreens, double actualSales,
            double actualInventory, double actualInventoryS, double actualInventoryN, double actualInventoryD, double actualInventoryE, double actualInventoryOF, double actualInventoryExternal, bool isActual)
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
            IsActual = isActual;
        }

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
        public bool IsActual { get; set; }
        public int ThisAbsWeekNumber => AbsWeekNumber(YearP, WeekNumber);

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

        public static int WeekQuarter(int week)
        {
            if (week <= 13) return 1;
            if (week <= 26) return 2;
            if (week <= 39) return 3;
            return 4;
        }

        public static int CurrentQuarter() { return WeekQuarter(DateWeek(DateTime.Today)); }

        public static int AbsWeekNumber(int year, int week) { return year * 100 + week; }

        public static int AbsWeekNumber(DateTime date) { return AbsWeekNumber(DateYear(date), DateWeek(date)); }

        public static int PrevAbsWeekNumber(int absWeekNumber)
        {
            if (absWeekNumber % 100 == 1) return absWeekNumber - 101 + NumOfWeeksInYear(absWeekNumber / 100 - 1);
            return absWeekNumber - 1;
        }

        public static int NextAbsWeekNumber(int absWeekNumber)
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
                if (NextAbsWeekNumber(minWeek) >= maxWeek) return new DbUpdates("UpdateWeeks", "Done", 0, 0);
                List<Weeks> ErpData = ErpDBServices.GetErpWeeks(minWeek, maxWeek);

                DbUpdates result = DBServices.AddWeeks(ErpData);
                result.FunctionName = "UpdateWeeks";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdateWeeks", "Error"); }
        }

        public static List<Weeks> GetInventoryForecast(int yearInt, Distribution dist = null, List<Periods> periodsList = null)
        {
            int year = yearInt == 0 ? CurrentYear() : yearInt;
            List<Weeks> result = new List<Weeks>();
            Dictionary<int, Weeks> actualWeeks = DBServices.GetYearWeeks(year);
            if (actualWeeks.Count == NumOfWeeksInYear(year)) return actualWeeks.Values.ToList();
            Weeks LastWeek = DBServices.GetLastWeeks();
            Dictionary<int, Periods> periods = DBServices.GetPeriods(Periods.PeriodNumber(LastWeek.YearP, LastWeek.QuarterP));
            if (!(periodsList is null)) for (int i = 0; i < periodsList.Count; i++) periods[periodsList[i].ThisPeriodNumber] = periodsList[i];
            int start = NextAbsWeekNumber(LastWeek.ThisAbsWeekNumber);
            Dictionary<int, ForecastSum> forecast = ForecastSum.GetForecastSum(LastWeek.YearP, year);
            Distribution distribution;
            if (dist is null) distribution = Distribution.GetDistribution();
            else distribution = dist;
            for (int i = AbsWeekNumber(year, 1); i < start; i = NextAbsWeekNumber(i)) result.Add(actualWeeks[i]);
            Weeks PrevWeek = LastWeek;
            for (int i = start; i <= AbsWeekNumber(year, NumOfWeeksInYear(year)); i = NextAbsWeekNumber(i))
            {
                Weeks tmp = new Weeks();
                tmp.WeekNumber = i % 100;
                tmp.YearP = i / 100;
                tmp.QuarterP = WeekQuarter(tmp.WeekNumber);
                tmp.IsActual = false;
                int pNumber = Periods.PeriodNumber(tmp.YearP, tmp.QuarterP);
                int numOfWeeks = Periods.NumOfWeeks(tmp.YearP, tmp.QuarterP);
                tmp.ActualInventoryS = Math.Round(
                    PrevWeek.ActualInventoryS +
                    Convert.ToDouble(periods[pNumber].DailyProduction) * 7 * distribution.S -
                    (forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseS) : 0) / numOfWeeks -
                    Convert.ToDouble(periods[pNumber].FactorS) / numOfWeeks
                    , 1);
                tmp.ActualInventoryN = Math.Round(
                    PrevWeek.ActualInventoryN +
                    Convert.ToDouble(periods[pNumber].DailyProduction) * 7 * distribution.N -
                    (forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseN) : 0) / numOfWeeks -
                    Convert.ToDouble(periods[pNumber].FactorN) / numOfWeeks
                    , 1);
                double subResL = Math.Round(
                    (forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseL) : 0) / numOfWeeks -
                    Convert.ToDouble(periods[pNumber].GreensUsing) +
                    Convert.ToDouble(periods[pNumber].FactorL) / numOfWeeks
                    , 1);
                subResL = subResL > 0 ? subResL : 0;
                double subResE = Math.Round(
                    PrevWeek.ActualInventoryE +
                    Convert.ToDouble(periods[pNumber].DailyProduction) * 7 * distribution.E -
                    (forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseE) : 0) / numOfWeeks -
                    Convert.ToDouble(periods[pNumber].FactorE) / numOfWeeks -
                    subResL
                    , 1);
                tmp.ActualInventoryE = subResE > 0 ? subResE : 0;
                double subResOF = Math.Round(
                    PrevWeek.ActualInventoryOF +
                    Convert.ToDouble(periods[pNumber].DailyProduction) * 7 * distribution.OF -
                    (forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseOF) : 0) / numOfWeeks -
                    Convert.ToDouble(periods[pNumber].FactorOF) / numOfWeeks +
                    (subResE < 0 ? subResE : 0)
                    , 1);
                tmp.ActualInventoryOF = subResOF > 0 ? subResOF : 0;
                tmp.ActualInventoryD = Math.Round(
                    PrevWeek.ActualInventoryD +
                    Convert.ToDouble(periods[pNumber].DailyProduction) * 7 * distribution.D -
                    (forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseD) : 0) / numOfWeeks -
                    Convert.ToDouble(periods[pNumber].FactorD) / numOfWeeks +
                    (subResOF < 0 ? subResE : 0)
                    , 1);
                tmp.ActualInventory = Math.Round(tmp.ActualInventoryS + tmp.ActualInventoryN + tmp.ActualInventoryD + tmp.ActualInventoryE + tmp.ActualInventoryOF, 1);
                tmp.ActualProduction = Convert.ToDouble(periods[pNumber].DailyProduction) * 7;
                tmp.ActualGreens = Convert.ToDouble(periods[pNumber].GreensUsing);
                tmp.ActualSales = Math.Round((forecast.Keys.Contains(pNumber) ? Convert.ToDouble(forecast[pNumber].FructoseS + forecast[pNumber].FructoseN + forecast[pNumber].FructoseD +
                    forecast[pNumber].FructoseE + forecast[pNumber].FructoseOF + forecast[pNumber].FructoseL) : 0) / numOfWeeks + Convert.ToDouble(periods[pNumber].FactorS +
                    periods[pNumber].FactorN + periods[pNumber].FactorD + periods[pNumber].FactorE + periods[pNumber].FactorOF + periods[pNumber].FactorL) / numOfWeeks, 1);
                double subEx = tmp.ActualInventory - periods[pNumber].StorageCapacity - periods[pNumber].ConteinersInventory;
                tmp.ActualInventoryExternal = Math.Round(subEx > 0 ? subEx : 0, 1);
                if (tmp.YearP == year) result.Add(tmp);
                PrevWeek = tmp;
            }
            return result;
        }
    }
}