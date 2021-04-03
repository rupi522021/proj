using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class PermissionTypes
    {
        public PermissionTypes(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public PermissionTypes() { }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}