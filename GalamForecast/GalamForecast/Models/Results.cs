using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace GalamForecast.Models
{
    public class Results
    {
        public Results(HttpStatusCode status, object returnedObj)
        {
            Status = status;
            ReturnedObj = returnedObj;
        }

        public HttpStatusCode Status { get; set; }
        public object ReturnedObj { get; set; }

        public static Results AuthenticationExResult(Exception ex)
        {
            if (ex.Message == "BadRequest") return new Results(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "בקשה שגויה" });
            if (ex.Message == "BadToken") return new Results(HttpStatusCode.NotAcceptable, new { Code = "BadToken", Message = "אתה לא מחובר למערכת" });
            if (ex.Message == "NoSession") return new Results(HttpStatusCode.NotAcceptable, new { Code = "NoSession", Message = "אתה לא מחובר למערכת" });
            return new Results(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
        }
    }
}