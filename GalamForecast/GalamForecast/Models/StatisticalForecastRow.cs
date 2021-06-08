using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace GalamForecast.Models
{
    public class StatisticalForecastRow
    {
        public static int rank0; //6
        public static int rank1; //8
        public static int rank2; //10
        public static int rank3; //12
        public static double[,] CF; //Coefficient Matrix //1-4

        static StatisticalForecastRow()
        {
            try
            {
                var fileSavePath = Path.Combine(HostingEnvironment.MapPath("~/ConstFiles"), "consts.txt");
                string[] readText = File.ReadAllLines(fileSavePath);
                rank0 = Convert.ToInt32(readText[6]);
                rank1 = Convert.ToInt32(readText[8]);
                rank2 = Convert.ToInt32(readText[10]);
                rank3 = Convert.ToInt32(readText[12]);
                CF = new double[4, 9];
                for (int i = 1; i <= 4; i++)
                {
                    string[] tmp = readText[i].Split(' ');
                    for (int j = 0; j < 9; j++)
                    {
                        try { CF[i - 1, j] = Convert.ToDouble(tmp[j].Replace(",", ".")); }
                        catch { CF[i - 1, j] = Convert.ToDouble(tmp[j].Replace(".", ",")); }
                    }
                }
            }
            catch
            {
                rank0 = 0;
                rank1 = 200;
                rank2 = 400;
                rank3 = 800;
                CF = new double[4, 9]{ { 0.05, 0, 0.31, 0.2, 0.2, 0.17, 0, 0, 0 },
                    { 0.51, 0.17, 0, -0.08, -0.02, 0.16, 0.21, 0.11, 0 },
                    { 0.79, 0, -0.14, 0, 0, 0.12, 0.27, 0, 0 },
                    { 0.46, 0.15, 0.21, 0, 0, 0.02, 0.22, 0, 0 }
                };
            }
        }

        public StatisticalForecastRow() { }

        public StatisticalForecastRow(int year, int customerNumber, string productFamilyName, string customerName, string personFullName, double[,] val)
        {
            Year = year;
            CustomerNumber = customerNumber;
            ProductFamilyName = productFamilyName;
            CustomerName = customerName;
            PersonFullName = personFullName;

            double avg1 = Math.Round((val[0, 1] + val[1, 1] + val[2, 1] + val[3, 1]) / 4, 3);
            double avg2 = Math.Round((val[0, 2] + val[1, 2] + val[2, 2] + val[3, 2]) / 4, 3);
            double avg3 = Math.Round((val[0, 3] + val[1, 3] + val[2, 3] + val[3, 3]) / 4, 3);
            double avg4 = Math.Round((val[0, 4] + val[1, 4] + val[2, 4] + val[3, 4]) / 4, 3);

            Val = new double[4, 9]{ { val[0, 0], val[0, 1], val[0, 2], val[0, 3], val[0, 4], avg1, avg2, avg3, avg4 },
                { val[1, 0], val[1, 1], val[1, 2], val[1, 3], val[1, 4],  avg1, avg2, avg3, avg4 },
                { val[2, 0], val[2, 1], val[2, 2], val[2, 3], val[2, 4],  avg1, avg2, avg3, avg4 },
                { val[3, 0], val[3, 1], val[3, 2], val[3, 3], val[3, 4],  avg1, avg2, avg3, avg4 }
            };
        }

        public int Year { get; set; }
        public int CustomerNumber { get; set; }
        public string ProductFamilyName { get; set; }
        public string CustomerName { get; set; }
        public string PersonFullName { get; set; }
        public double Q1 => SForc(1);
        public double Q2 => SForc(2);
        public double Q3 => SForc(3);
        public double Q4 => SForc(4);
        public double Total => Math.Round(Q1 + Q2 + Q3 + Q4, 1);
        public string StringKey => $"{ProductFamilyName}|{Year}|{CustomerNumber}";
        private double[,] Val;  //Values Matrix

        public static int RCF(double num) //4 Coefficient Matrix Row
        {
            if (num > rank0 && num <= rank1) return 1;
            if (num > rank1 && num <= rank2) return 2;
            if (num > rank2 && num <= rank3) return 3;
            return 4;
        }

        private int IZ(double num)
        {
            if (num == 0) return 0;
            return 1;
        }

        private double SForc(int q)
        {
            if (Val[q - 1, 0] == 0) return 0;
            int row = RCF(Val[q - 1, 0]);
            double numerator = 0;
            double denominator = 0;
            double sumCF = 0;
            for (int i = 0; i < 9; i++)
            {
                numerator += Val[q - 1, i] * CF[row - 1, i];
                denominator += IZ(Val[q - 1, i]) * CF[row - 1, i];
                sumCF += CF[row - 1, i];
            }
            return Math.Round(sumCF * numerator / denominator, 2);
        }

        public static List<int> GetStatisticalForecastYears() { return DBServices.GetStatisticalForecastYears(); }

        public static List<StatisticalForecastRow> GetStatisticalForecastRows(int year) { return DBServices.GetStatisticalForecastRows(year == 0 ? Weeks.CurrentYear() : year); }
    }
}