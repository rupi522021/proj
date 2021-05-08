using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class InventoryController : ApiController
    {
        [HttpGet]
        [Route("api/InventoryYears/")]
        public IHttpActionResult InventoryYearsGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            try
            {
                List<int> result = InventoryRows.GetInventoryYears();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Years = result, CurrentYear = Weeks.CurrentYear() });
            }
            catch { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" }); }
        }

        [HttpGet]
        [Route("api/InventoryParams/")]
        public IHttpActionResult InventoryParamsGet(string year = "0")
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            int yearInt;
            try { yearInt = Convert.ToInt32(year); }
            catch { return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שנה לא תקינה" }); }

            try
            {
                List<Periods> periods = Periods.GetPeriods(yearInt);
                Distribution distribution = Distribution.GetDistribution();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Periods = periods, Distribution = distribution, Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתונים", Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/InventoryParams/")]
        public IHttpActionResult Post([FromBody]List<Periods> periods, string S = null, string N = null, string D = null, string E = null, string OF = null)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(5)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            Dictionary<string, double> distD = new Dictionary<string, double>();
            try
            {
                if (!(S is null)) distD["S"] = Convert.ToDouble(S.Replace(",", "."));
                if (!(N is null)) distD["N"] = Convert.ToDouble(N.Replace(",", "."));
                if (!(D is null)) distD["D"] = Convert.ToDouble(D.Replace(",", "."));
                if (!(E is null)) distD["E"] = Convert.ToDouble(E.Replace(",", "."));
                if (!(OF is null)) distD["OF"] = Convert.ToDouble(OF.Replace(",", "."));
            }
            catch (Exception ex)
            {
                try
                {
                    distD = new Dictionary<string, double>();
                    if (!(S is null)) distD["S"] = Convert.ToDouble(S.Replace(".", ","));
                    if (!(N is null)) distD["N"] = Convert.ToDouble(N.Replace(".", ","));
                    if (!(D is null)) distD["D"] = Convert.ToDouble(D.Replace(".", ","));
                    if (!(E is null)) distD["E"] = Convert.ToDouble(E.Replace(".", ","));
                    if (!(OF is null)) distD["OF"] = Convert.ToDouble(OF.Replace(".", ","));
                }
                catch (Exception ext) { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "פילוג לא תקין SH" }); }
            }


            try
            {
                if (InventoryRows.PostInventoryParams(distD, periods is null ? new List<Periods>() : periods)) return Content(HttpStatusCode.OK, new { Code = "OK", Message = "עדכון בוצע" });
                else return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/InventoryForecast/")]
        public IHttpActionResult InventoryForecastGet(string year = "0")
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            int yearInt;
            try { yearInt = Convert.ToInt32(year); }
            catch { return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שנה לא תקינה" }); }

            try
            {
                List<Weeks> result = Weeks.GetInventoryForecast(yearInt);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", InventoryForecast = result, Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא ניתן להציג תחזית מלאי", Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/SimulationInventory/")]
        public IHttpActionResult SimulationInventoryPost([FromBody]List<Periods> periods, string S, string N, string D, string E, string OF, string year)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            int yearInt;
            try { yearInt = Convert.ToInt32(year); }
            catch { return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שנה לא תקינה" }); }

            Distribution distribution;
            try
            {
                distribution = new Distribution(
                     Convert.ToDouble(S.Replace(",", ".")),
                     Convert.ToDouble(N.Replace(",", ".")),
                     Convert.ToDouble(D.Replace(",", ".")),
                     Convert.ToDouble(E.Replace(",", ".")),
                     Convert.ToDouble(OF.Replace(",", "."))
                    );
            }
            catch (Exception ex)
            {
                try
                {
                    distribution = new Distribution(
                     Convert.ToDouble(S.Replace(".", ",")),
                     Convert.ToDouble(N.Replace(".", ",")),
                     Convert.ToDouble(D.Replace(".", ",")),
                     Convert.ToDouble(E.Replace(".", ",")),
                     Convert.ToDouble(OF.Replace(".", ","))
                    );
                }
                catch (Exception ext) { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "פילוג לא תקין SH" }); }
            }


            try
            {
                List<Weeks> result = Weeks.GetInventoryForecast(yearInt, distribution, periods is null ? new List<Periods>() : periods);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", InventoryForecast = result, Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא ניתן להציג תחזית מלאי", Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}