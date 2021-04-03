using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class UserTypes
    {
        public UserTypes(int id, string name, List<PermissionTypes> permissionTypes)
        {
            Id = id;
            Name = name;
            PermissionTypes = permissionTypes;
        }

        public UserTypes(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public UserTypes() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<PermissionTypes> PermissionTypes { get; set; }
    }
}