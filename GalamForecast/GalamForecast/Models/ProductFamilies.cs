using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class ProductFamilies
    {
        public ProductFamilies(string id, string productFamilyName)
        {
            Id = id;
            ProductFamilyName = productFamilyName;
        }

        public ProductFamilies() { }

        public string Id { get; set; }
        public string ProductFamilyName { get; set; }

        public static List<ProductFamilies> GetAllProductFamilies() { return DBServices.GetAllProductFamilies(); }
    }
}