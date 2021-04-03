using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Periods
    {
        const int defaultDailyProduction = 340;
        const int defaultGreensUsing = 0;
        const int defaultStorageCapacity = 6000;
        const int defaultConteinersInventory = 1200;
        const int firstYear = 2016;

        public Periods() { }

        public Periods(int periodNumber) : this(firstYear + (periodNumber - 1) / 4, (periodNumber - 1) % 4 + 1) { }

        public Periods(int yearP, int quarterP, int dailyProduction, int greensUsing, int factorS, int factorN, int factorD, int factorE, int factorOF, int factorL, int storageCapacity, int conteinersInventory)
        {
            YearP = yearP;
            QuarterP = quarterP;
            DailyProduction = dailyProduction;
            GreensUsing = greensUsing;
            FactorS = factorS;
            FactorN = factorN;
            FactorD = factorD;
            FactorE = factorE;
            FactorOF = factorOF;
            FactorL = factorL;
            StorageCapacity = storageCapacity;
            ConteinersInventory = conteinersInventory;
        }

        public Periods(int yearP, int quarterP)
        {
            YearP = yearP;
            QuarterP = quarterP;
            DailyProduction = defaultDailyProduction;
            GreensUsing = defaultGreensUsing;
            FactorS = 0;
            FactorN = 0;
            FactorD = 0;
            FactorE = 0;
            FactorOF = 0;
            FactorL = 0;
            StorageCapacity = defaultStorageCapacity;
            ConteinersInventory = defaultConteinersInventory;
        }

        public int YearP { get; set; }
        public int QuarterP { get; set; }
        public int DailyProduction { get; set; }
        public int GreensUsing { get; set; }
        public int FactorS { get; set; }
        public int FactorN { get; set; }
        public int FactorD { get; set; }
        public int FactorE { get; set; }
        public int FactorOF { get; set; }
        public int FactorL { get; set; }
        public int StorageCapacity { get; set; }
        public int ConteinersInventory { get; set; }
        public int ThisPeriodNumber => PeriodNumber(YearP, QuarterP);

        public static int Qarter(DateTime date) { return (date.Month - 1) / 3 + 1; }

        public static int PeriodNumber(int year, int quarter) { return (year - firstYear) * 4 + quarter; }

        public static int PeriodNumber(DateTime date) { return (date.Year - firstYear) * 4 + Qarter(date); }

        public static DbUpdates AddNewPeriods()
        {
            try
            {
                List<int> dbPeriodNumbers = new List<int>();
                dbPeriodNumbers.AddRange(DBServices.GetAllPeriods().Select(item => item.ThisPeriodNumber));
                List<Periods> periodsToAdd = new List<Periods>();
                for (int i = 1; i < PeriodNumber(DateTime.Today.Year + 2, 1); i++) { if (!dbPeriodNumbers.Contains(i)) periodsToAdd.Add(new Periods(i)); }
                if (periodsToAdd.Count > 0) return new DbUpdates("AddNewPeriods", "Done", 0, DBServices.AddPeriods(periodsToAdd));
                else return new DbUpdates("AddNewPeriods", "Done", 0, 0);
            }
            catch { return new DbUpdates("AddNewPeriods", "Error", 0, 0); }
        }
    }
}