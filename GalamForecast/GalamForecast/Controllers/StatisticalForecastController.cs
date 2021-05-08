using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class StatisticalForecastController : ApiController
    {
        public IHttpActionResult Get(string year = "0")
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(7)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            int yearInt;
            try { yearInt = Convert.ToInt32(year); }
            catch { return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שנה לא תקינה" }); }

            try
            {
                List<StatisticalForecastRow> result = StatisticalForecastRow.GetStatisticalForecastRows(yearInt);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", StatisticalForecast = result, Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתוני תחזית", Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/StatisticalForecastYears/")]
        public IHttpActionResult StatisticalForecastYearsGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(7)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<int> result = StatisticalForecastRow.GetStatisticalForecastYears();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Years = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתוני תחזית" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}