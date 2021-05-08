using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Distribution
    {
        public Distribution(double s, double n, double d, double e, double oF)
        {
            S = s;
            N = n;
            D = d;
            E = e;
            OF = oF;
        }

        public Distribution() { }

        public double S { get; set; }
        public double N { get; set; }
        public double D { get; set; }
        public double E { get; set; }
        public double OF { get; set; }

        public static Distribution GetDistribution() { return DBServices.GetDistribution(); }
    }
}