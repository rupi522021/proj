using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class ShipTosController : ApiController
    {
        public IHttpActionResult Get()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(6) && !(user.Permissions.Contains(1) || user.Permissions.Contains(2) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<ShipTos> result = ShipTos.GetShipTos();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", ShipTos = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו לקוחות קצה" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        public IHttpActionResult Post([FromBody]ShipTos ShipTo)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(6)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                if (ShipTos.UpdateShipToActive(ShipTo)) return Content(HttpStatusCode.OK, new { Code = "OK", Message = "לקוח קצה עודכן", ShipTo });
                return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח", ShipTo });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לקוח קצה לא קיים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/countriesGet/")]
        public IHttpActionResult countriesGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(6)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<Countries> result = Countries.GetCountries();
                if (result.Count==0) return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו מדינות" });
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Countries = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        public IHttpActionResult Put([FromBody]ShipTos ShipTo)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.Permissions.Contains(6)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                ShipTos result = ShipTos.AddShipTo(ShipTo);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "לקוח קצה הוקם", ShipTo = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "Exists") return Content(HttpStatusCode.BadRequest, new { Code = "Exists", Message = "לקוח קצה כבר קיים" });
                if (ex.Message == "CustomerNotFound") return Content(HttpStatusCode.NotFound, new { Code = "CustomerNotFound", Message = "לקוח לא קיים" });
                if (ex.Message == "CountryNotFound") return Content(HttpStatusCode.NotFound, new { Code = "CountryNotFound", Message = "מדינה לא קיימת" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}