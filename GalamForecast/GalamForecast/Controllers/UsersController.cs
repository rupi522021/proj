using GalamForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GalamForecast.Controllers
{
    public class UsersController : ApiController
    {
        [HttpGet]
        [Route("api/login/")]
        public IHttpActionResult logInGet(string UserName, string Password)
        {
            try
            {
                Users user = Users.LogIn(UserName.ToUpper(), Password);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "התחברות הצליחה", User = user });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "UserNotFound") return Content(HttpStatusCode.NotFound, new { Code = "UserNotFound", Message = "משתמש או סיסמה לא נכונים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/logout/")]
        public IHttpActionResult logOutGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            try
            {
                try { ForecastRows.ClearLockes(user); }
                catch { }
                ForecastRows.ClearLockes(user);
                if (Users.LogOut(user.UserName)) return Content(HttpStatusCode.OK, new { Code = "OK", Message = "אתה מנותק ממערכת" });
                else return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "התנתקות לא הצליחה" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NoSession", Message = "אתה מנותק ממערכת" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/numOfApprovals/")]
        public IHttpActionResult numOfApprovalsGet()
        {
            //Users user;
            //try { user = Users.Authentication(Request); }
            //catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            //if (!(user.Permissions.Contains(3)|| user.Permissions.Contains(4)|| user.IsSaleMenager)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            //Add

            return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", NumOfApprovals = 1 });
        }

        public IHttpActionResult Get()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<Users> result = Users.GetAllUsers();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "נשלחה תשובה", Users = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "לא נמצאו משתמשים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/lockUser/")]
        public IHttpActionResult lockUserPost([FromBody]string userName)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            if (userName == null) return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שם משתמש לחסימה לא תקין" });

            try
            {
                if (Users.LockUser(userName.ToUpper())) return Content(HttpStatusCode.OK, new { Code = "OK", Message = $"משתמש {userName.ToUpper()} נחסם", UserName = userName });
                else return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "UserNotFound") return Content(HttpStatusCode.NotFound, new { Code = "UserNotFound", Message = $"משתמש {userName.ToUpper()} לא קיים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/unLockUser/")]
        public IHttpActionResult unLockUserPost([FromBody]string userName)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            if (userName == null) return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "שם משתמש לחסימה לא תקין" });

            try
            {
                if (Users.UnLockUser(userName.ToUpper())) return Content(HttpStatusCode.OK, new { Code = "OK", Message = $"משתמש {userName.ToUpper()} שוחרר מחסימה", UserName = userName });
                else return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "UserNotFound") return Content(HttpStatusCode.NotFound, new { Code = "UserNotFound", Message = $"משתמש {userName.ToUpper()} לא קיים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/passZero/")]
        public IHttpActionResult passZeroPost([FromBody]Users userFromBody)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                if (Users.PassZero(userFromBody)) return Content(HttpStatusCode.OK, new { Code = "OK", Message = $"סיסמת משתמש {userFromBody.UserName.ToUpper()} אופסה בהצלחה", userFromBody.UserName });
                else return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "UserNotFound") return Content(HttpStatusCode.NotFound, new { Code = "UserNotFound", Message = $"משתמש {userFromBody.UserName.ToUpper()} לא קיים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/roolGet/")]
        public IHttpActionResult roolGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                object result = Users.RoolGet();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "משיכה הצליחה", Rools = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "בעיה במשיכת נתונים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/roolPost/")]
        public IHttpActionResult roolPost([FromBody]Users userFromBody)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                if (Users.RoolPost(userFromBody)) return Content(HttpStatusCode.OK, new { Code = "OK", Message = $"עדכון הרשאות משתמש {userFromBody.UserName.ToUpper()} בוצע בהצלחה", user = userFromBody });
                else return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = "עדכון לא הצליח" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "UserNotFound") return Content(HttpStatusCode.NotFound, new { Code = "UserNotFound", Message = $"משתמש {userFromBody.UserName.ToUpper()} לא קיים" });
                if (ex.Message == "BadRequest") return Content(HttpStatusCode.NotFound, new { Code = "BadRequest", Message = $"אין שינויים לעדכון" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpGet]
        [Route("api/peopleGet/")]
        public IHttpActionResult peopleGet()
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                List<int> result = Users.GetAllPeople();
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "משיכה הצליחה", People = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "NotFound") return Content(HttpStatusCode.NotFound, new { Code = "NotFound", Message = "בעיה במשיכת נתונים" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPut]
        [Route("api/addUser/")]
        public IHttpActionResult addUser([FromBody]Users userFromBody)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (!user.UserTypes.Contains(4)) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                Users result = Users.AddUser(userFromBody);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = $"הוספת משתמש {result.UserName.ToUpper()} הצליחה", user = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "BadRequest") return Content(HttpStatusCode.BadRequest, new { Code = "BadRequest", Message = $"הוספת משתמש {userFromBody.UserName.ToUpper()} לא הצליחה" });
                if (ex.Message == "UserInSystem") return Content(HttpStatusCode.BadRequest, new { Code = "UserInSystem", Message = $"משתמש {userFromBody.UserName.ToUpper()} כבר קיים במערכת" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }

        [HttpPost]
        [Route("api/changePassword/")]
        public IHttpActionResult changePassword(string UserName, string OldPassword, string NewPassword)
        {
            Users user;
            try { user = Users.Authentication(Request); }
            catch (Exception ex) { return Content(Results.AuthenticationExResult(ex).Status, Results.AuthenticationExResult(ex).ReturnedObj); }

            if (UserName!= user.UserName) return Content(HttpStatusCode.Forbidden, new { Code = "Forbidden", Message = "אין הרשאות" });

            try
            {
                Users result = Users.ChangePassword(UserName.ToUpper(), OldPassword, NewPassword);
                return Content(HttpStatusCode.OK, new { Code = "OK", Message = "התחברות הצליחה", User = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "con") return Content(HttpStatusCode.NotFound, new { Code = "con", Message = "מסד נתונים לא זמין" });
                if (ex.Message == "WrongPassword") return Content(HttpStatusCode.NotFound, new { Code = "WrongPassword", Message = "סיסמה ישנה לא תקינה" });
                if (ex.Message == "BadRequest") return Content(HttpStatusCode.NotFound, new { Code = "BadRequest", Message = "שינוי סיסמה לא הצליח" });
                return Content(HttpStatusCode.InternalServerError, new { Code = "ServerError", Message = "שגיאה בשרת" });
            }
        }
    }
}