using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Lockes
    {
        public Lockes(string lockedToUser, DateTime lockedDateTime)
        {
            LockedToUser = lockedToUser;
            LockedDateTime = lockedDateTime;
        }

        public Lockes() { }

        public string LockedToUser { get; set; }
        public DateTime LockedDateTime { get; set; }
    }
}