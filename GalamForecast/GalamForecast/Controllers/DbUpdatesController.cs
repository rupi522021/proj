using GalamForecast.Models;
using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class DbUpdatesController : ApiController
    {
        public IHttpActionResult Post()
        {
            try { return Content(HttpStatusCode.OK, new { Code = "OK", Message = "עדכון בסיס נתונים", result = DbUpdates.UpdateTables() }); }
            catch { return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" }); }
        }
    }
}