using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class ApprovalsController : ApiController
    {
        [HttpGet]
        [Route("api/numOfApprovals/")]
        public IHttpActionResult NumOfApprovalsGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(3) || user.Permissions.Contains(4) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                int result = ApprovalRows.GetNumOfApprovals(user);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", NumOfApprovals = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        public IHttpActionResult Get()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(3) || user.Permissions.Contains(4) || user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<ApprovalRows> result = ApprovalRows.GetApprovals(user);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Approvals = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        public IHttpActionResult Post(int id, string action)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!(user.Permissions.Contains(3) || user.Permissions.Contains(4) || user.IsSaleMenager) || (!(user.Permissions.Contains(3) || user.Permissions.Contains(4)) && action == "allow"))
                return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<ApprovalRows> result = ApprovalRows.PostApprovals(user, id, action);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Approvals = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאה בקשה" });
                if (ex.Message == "BadRequest") return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
                if (ex.Message == "Completed") return Content(HttpStatusCode.BadRequest, new { Code = "Completed", Message = "בקשה כבר אושרה על-ידי משתמש אחר. כדי לראות רשימה מעודכנת ניתן לבצע ריענון." });
                if (ex.Message == "Canceled") return Content(HttpStatusCode.BadRequest, new { Code = "Canceled", Message = "בקשה כבר בוטלה על-ידי משתמש אחר. כדי לראות רשימה מעודכנת ניתן לבצע ריענון." });
                if (ex.Message == "Forbidden") return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}