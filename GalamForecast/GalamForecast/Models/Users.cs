using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace GalamForecast.Models
{
    public class Users
    {
        public static Dictionary<string, Users> LogedInUsers;
        static Users() { LogedInUsers = new Dictionary<string, Users>(); }

        public Users(SqlDataReader dr)
        {
            UserName = Convert.ToString(dr["userName"]);
            Active = Convert.ToBoolean(dr["userActive"]);
            IsNewPassword = Convert.ToBoolean(dr["userIsNewPassword"]);
            if (!(dr["personId"] == DBNull.Value))
            {
                PersonId = Convert.ToInt32(dr["personId"]);
                if (dr["personFullName"] != DBNull.Value) FullName = Convert.ToString(dr["personFullName"]);
                if (dr["personEmail"] != DBNull.Value) Email = Convert.ToString(dr["personEmail"]);
                if (dr["isActiveSaleMenager"] != DBNull.Value) IsSaleMenager = Convert.ToBoolean(dr["isActiveSaleMenager"]);
            }
        }

        public Users(string userName, bool active, bool isNewPassword, string password, int personId, string fullName, string email, bool isSaleMenager, List<int> permissions, List<int> userTypes)
        {
            UserName = userName;
            Active = active;
            IsNewPassword = isNewPassword;
            Password = password;
            PersonId = personId;
            FullName = fullName;
            Email = email;
            IsSaleMenager = isSaleMenager;
            Permissions = permissions;
            UserTypes = userTypes;
        }

        public Users() { }

        public string UserName { get; set; }
        public bool Active { get; set; }
        public bool IsNewPassword { get; set; }
        public string Password { get; set; }
        public int PersonId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsSaleMenager { get; set; }
        public List<int> Permissions { get; set; }
        public List<int> UserTypes { get; set; }
        public string Token { get; set; }

        private static string genToken()
        {
            const int tokenLength = 128;
            Random rdm = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string result = "";
            for (int i = 0; i < tokenLength; i++) result += characters[rdm.Next(characters.Length)];
            return result;
        }

        public static Users LogIn(string UserName, string Password)
        {
            Users user = DBServices.LogIn(UserName, Password);
            user.Token = genToken();
            LogedInUsers[user.UserName] = user;
            return user;
        }

        public static Users Authentication(HttpRequestMessage req)
        {
            string Token;
            string UserName;
            try
            {
                Token = req.Headers.GetValues("X-Token").FirstOrDefault();
                UserName = req.Headers.GetValues("X-UserName").FirstOrDefault();
            }
            catch { throw new Exception("BadRequest"); }

            if (LogedInUsers.Keys.Contains(UserName))
            {
                if (LogedInUsers[UserName].Token == Token) return LogedInUsers[UserName];
                throw new Exception("BadToken");
            }
            else { throw new Exception("NoSession"); }
        }

        public static object AuthenticationExResult(Exception ex)
        {
            if (ex.Message == "BadRequest") return new { status = HttpStatusCode.BadRequest, returnedObj = new { Code = "BadRequest", Message = "בקשה שגויה" } };
            if (ex.Message == "BadToken") return new { status = HttpStatusCode.NotAcceptable, returnedObj = new { Code = "BadToken", Message = "אתה לא מחובר למערכת" } };
            if (ex.Message == "NoSession") return new { status = HttpStatusCode.NotAcceptable, returnedObj = new { Code = "NoSession", Message = "אתה לא מחובר למערכת" } };
            return new { status = HttpStatusCode.InternalServerError, returnedObj = new { Code = "ServerError", Message = "שגיאה בשרת" } };
        }

        public static bool LogOut(string UserName)
        {
            if (LogedInUsers.Keys.Contains(UserName)) LogedInUsers.Remove(UserName);
            else throw new Exception("NotFound");
            if (LogedInUsers.Keys.Contains(UserName)) return false;
            else return true;
        }

        public static List<Users> GetAllUsers() { return DBServices.GetAllUsers(); }

        public static bool LockUser(string UserName)
        {
            if (DBServices.LockUser(UserName))
            {
                if (LogedInUsers.Keys.Contains(UserName)) LogedInUsers.Remove(UserName);
                return true;
            }
            else return false;
        }

        public static bool UnLockUser(string UserName) { return DBServices.UnLockUser(UserName); }

        public static bool PassZero(Users user) { return DBServices.PassZero(user); }

        public static object RoolGet()
        {
            List<PermissionTypes> PermissionTypes = DBServices.GetAllPermissionTypes();
            List<UserTypes> UserTypes = DBServices.GetAllUserTypes();
            return new { PermissionTypes, UserTypes };
        }

        public static bool RoolPost(Users user) { return DBServices.RoolPost(user); }

        public static List<int> GetAllPeople() { return DBServices.GetAllPeople(); }

        public static Users AddUser(Users user) { return DBServices.AddUser(user); }

        public static Users ChangePassword(string UserName, string OldPassword, string NewPassword)
        {
            if (DBServices.ChangePassword(UserName, OldPassword, NewPassword)) LogedInUsers[UserName].IsNewPassword = false;
            return LogedInUsers[UserName];
        }
    }
}