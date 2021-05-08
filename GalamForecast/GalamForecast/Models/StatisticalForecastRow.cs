using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class StatisticalForecastRow
    {
        public StatisticalForecastRow() { }

        public StatisticalForecastRow(int year, int customerNumber, string productFamilyName, string customerName, string personFullName,
            double q1F, double q2F, double q3F, double q4F, double q11, double q21, double q31, double q41, double q12, double q22,
            double q32, double q42, double q13, double q23, double q33, double q43, double q14, double q24, double q34, double q44)
        {
            Year = year;
            CustomerNumber = customerNumber;
            ProductFamilyName = productFamilyName;
            CustomerName = customerName;
            PersonFullName = personFullName;

            double avg1 = Math.Round((q11 + q21 + q31 + q41) / 4, 3);
            double avg2 = Math.Round((q12 + q22 + q32 + q42) / 4, 3);
            double avg3 = Math.Round((q13 + q23 + q33 + q43) / 4, 3);
            double avg4 = Math.Round((q14 + q24 + q34 + q44) / 4, 3);

            Val = new double[4, 9]{ { q1F, q11, q12, q13, q14, avg1, avg2, avg3, avg4 },
                { q2F, q21, q22, q23, q24,  avg1, avg2, avg3, avg4 },
                { q3F, q31, q32, q33, q34,  avg1, avg2, avg3, avg4 },
                { q4F, q41, q42, q43, q44,  avg1, avg2, avg3, avg4 }
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
        private double[,] CF = { { 0.05, 0, 0.31, 0.2, 0.2, 0.17, 0, 0, 0 },
                { 0.51, 0.17, 0, -0.08, -0.02, 0.16, 0.21, 0.11, 0 },
                { 0.79, 0, -0.14, 0, 0, 0.12, 0.27, 0, 0 },
                { 0.46, 0.15, 0.21, 0, 0, 0.02, 0.22, 0, 0 }
            };  //Coefficient Matrix

        public static int RCF(double num) //4 Coefficient Matrix Row
        {
            if (num > 0 && num <= 200) return 1;
            if (num > 200 && num <= 400) return 2;
            if (num > 400 && num <= 800) return 3;
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