using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class TransactionsController : ApiController
    {
        [HttpGet]
        [Route("api/TransactionsDimensions/")]
        public IHttpActionResult StatisticalForecastYearsGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            try
            {
                List<int> years = ForecastRows.GetForecastYears();
                List<ShipTos> shipTos=new List<ShipTos>();

                try { shipTos = ShipTos.GetShipTos(); }
                catch (Exception ex) { if (ex.Message != "NotFound") throw ex; }

                List<Countries> countries = Countries.GetCountries();
                List<Items> items = Items.GetAllItems();
                List<ProductFamilies> productFamilies = ProductFamilies.GetAllProductFamilies();

                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Years = years, ShipTos = shipTos, Countries = countries, Items = items, ProductFamilies = productFamilies });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתונים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        public IHttpActionResult Get(string from = null, string to = null, string year = null, string quarter = null,
            string customerNumber = null, string shipToName = null, string itemNumber = null, string productFamilyId = null, string market = null, string countryId = null)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            try
            {
                List<Transactions> result = Transactions.GetTransactions(from, to, year, quarter, customerNumber, shipToName, itemNumber, productFamilyId, market, countryId);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Transactions = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו תנועות לפי פרמטרים שהוזנו" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}