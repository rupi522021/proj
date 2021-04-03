using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class ForecastController : ApiController
    {
        public IHttpActionResult Get(string year = "0", string fDate = null)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            int yearInt;
            try { yearInt = Convert.ToInt32(year); }
            catch { return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שנה לא תקינה" }); }

            try
            {
                List<ForecastRows> result = ForecastRows.GetForecast(yearInt, fDate);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Forecast = result, Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt, FDate = fDate });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתוני תחזית", Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt, FDate = fDate });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/ForecastYears/")]
        public IHttpActionResult ForecastYearsGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            try
            {
                List<int> result = ForecastRows.GetForecastYears();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Years = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתוני תחזית" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/ForecastUpdateYears/")]
        public IHttpActionResult ForecastUpdateYears()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<int> result = ForecastRows.GetForecastUpdateYears();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Years = result });
            }
            catch { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" }); }
        }

        [HttpGet]
        [Route("api/ForecastToUpdate/")]
        public IHttpActionResult GetForecastToUpdate(string year = "0")
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            int yearInt;
            try { yearInt = Convert.ToInt32(year); }
            catch { return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שנה לא תקינה" }); }

            try
            {
                List<ForecastRows> result = ForecastRows.GetForecastToUpdate(yearInt, user);
                List<int> lockedQuarters = ForecastRows.GetLockedQuarters(yearInt);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", RowsToUpdate = result, LockedQuarters = lockedQuarters, Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
            }
            catch (Exception ex)
            {
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו נתוני תחזית", Year = yearInt == 0 ? Weeks.CurrentYear() : yearInt });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/GetLockes/")]
        public IHttpActionResult GetLockes()
        {
            try
            {
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", ForecastRows.LockedForecastRows, ForecastRows.LockedCustomerNumbers });
            }
            catch
            {
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpDelete]
        [Route("api/ClearLockes/")]
        public IHttpActionResult ClearLockes()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "בוצע ניקוי נעילות", Done = ForecastRows.ClearLockes(user) });
            }
            catch
            {
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        public IHttpActionResult Post([FromBody]List<ForecastRows> forecastRowFromClient)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                if (forecastRowFromClient.Count == 0) return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "אין שורות לעדכון" });
                if (ForecastRows.UpdateForecast(user, forecastRowFromClient)) return Content(HttpStatusCode.OK, new { Code = "OK", Message = "עדכון בוצע" });
                else return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "אין שורות לעדכון" });
            }
            catch { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" }); }
        }

        [HttpGet]
        [Route("api/Items/")]
        public IHttpActionResult GetAllItems()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<Items> result = Items.GetAllItems();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Items = result });
            }
            catch { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" }); }
        }

        public IHttpActionResult Put([FromBody]ForecastRows row)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                ForecastRows result = ForecastRows.AddNewForecastRow(user, row);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "שורת תחזית נוספה", UpdateStatus = "Completed", ForecastRow = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "Exists") return Content(HttpStatusCode.BadRequest, new { Code = "Exists", Message = "שורת תחזית כבר קיימת" });
                if (ex.Message == "CustomerNotFound") return Content(HttpStatusCode.NotFound, new { Code = "CustomerNotFound", Message = "לקוח לא קיים" });
                if (ex.Message == "Forbidden") return Content(HttpStatusCode.NotFound, new { Code = "Forbidden", Message = "אין הרשאה לעדכן תחזית עבור לקוח זה" });
                if (ex.Message == "BadRequest") return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
                if (ex.Message == "Pending") return Content(HttpStatusCode.OK, new { Code = "OK", Message = "שורת תחזית נוספה", UpdateStatus = "Pending" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}