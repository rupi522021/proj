using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace GalamForecast.Models.DAL
{
    public class DBServices
    {
        private static SqlConnection connect(string conString = "DBConnectionString")
        {
            try {
                string cStr = WebConfigurationManager.ConnectionStrings[conString].ConnectionString;
                SqlConnection con = new SqlConnection(cStr);
                con.Open();
                return con;
            }
            catch { throw new Exception("con"); }
        }

        private static SqlCommand CreateCommand(string CommandSTR, SqlConnection con, int timeout = 10)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = CommandSTR;
            cmd.CommandTimeout = timeout;
            cmd.CommandType = System.Data.CommandType.Text;
            return cmd;
        }

        public static Users LogIn(string UserName, string Password)
        {
            SqlConnection con = connect("DBConnectionString");
            try
            {
                if (UserName == "SYSTEM") throw new Exception("UserNotFound");
                string str = $"select userName, tblUsers.personId as personId, userPasswordHash, userPasswordSalt, userActive, userIsNewPassword, userPassword, personFullName, personEmail, isActiveSaleMenager" +
                    $" from  tblUsers left join tblPeople on tblUsers.personId = tblPeople.personId left join tblSaleMenager" +
                    $" on tblUsers.personId = tblSaleMenager.personId where userName = '{UserName}' and userPassword = '{Password}' and userActive = 1";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("UserNotFound"); }
                dr.Read();
                Users result = new Users(dr);
                dr.Close();
                str = $"select tblUsersVsUserTypes.userTypeId, tblUserTypesVsPermissionTypes.permissionTypeId as permissionByUserType, tblUsersVsPermissionTypes.permissionTypeId as permissionByUser" +
                    $" from tblUsersVsUserTypes left join tblUserTypesVsPermissionTypes on tblUsersVsUserTypes.userTypeId = tblUserTypesVsPermissionTypes.userTypeId" +
                    $" left join tblUsersVsPermissionTypes on tblUsersVsUserTypes.userName = tblUsersVsPermissionTypes.userName where tblUsersVsUserTypes.userName = '{UserName}'";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                List<int> typesList = new List<int>();
                List<int> permissionsList = new List<int>();
                while (dr.Read())
                {
                    if (dr["userTypeId"] != DBNull.Value) if (!typesList.Contains(Convert.ToInt32(dr["userTypeId"]))) typesList.Add(Convert.ToInt32(dr["userTypeId"]));
                    if (dr["permissionByUser"] != DBNull.Value) if (!permissionsList.Contains(Convert.ToInt32(dr["permissionByUser"]))) permissionsList.Add(Convert.ToInt32(dr["permissionByUser"]));
                    if (dr["permissionByUserType"] != DBNull.Value) if (!permissionsList.Contains(Convert.ToInt32(dr["permissionByUserType"]))) permissionsList.Add(Convert.ToInt32(dr["permissionByUserType"]));
                }
                result.UserTypes = typesList;
                result.Permissions = permissionsList;
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Users> GetAllUsers()
        {
            SqlConnection con = connect("DBConnectionString");
            List<Users> result = new List<Users>();

            try
            {
                string str = $"select userName, tblUsers.personId as personId, userPasswordHash, userPasswordSalt, userActive, userIsNewPassword, userPassword, personFullName, personEmail, isActiveSaleMenager" +
                    $" from  tblUsers left join tblPeople on tblUsers.personId = tblPeople.personId left join tblSaleMenager" +
                    $" on tblUsers.personId = tblSaleMenager.personId where userName<>'SYSTEM'";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                while (dr.Read()) { result.Add(new Users(dr)); }
                dr.Close();
                List<int> typesList;
                List<int> permissionsList;
                foreach (Users user in result)
                {
                    str = $"select tblUsersVsUserTypes.userTypeId, tblUserTypesVsPermissionTypes.permissionTypeId as permissionByUserType, tblUsersVsPermissionTypes.permissionTypeId as permissionByUser" +
                    $" from tblUsersVsUserTypes left join tblUserTypesVsPermissionTypes on tblUsersVsUserTypes.userTypeId = tblUserTypesVsPermissionTypes.userTypeId" +
                    $" left join tblUsersVsPermissionTypes on tblUsersVsUserTypes.userName = tblUsersVsPermissionTypes.userName where tblUsersVsUserTypes.userName = '{user.UserName}'";
                    cmd = CreateCommand(str, con);
                    dr = cmd.ExecuteReader();
                    typesList = new List<int>();
                    permissionsList = new List<int>();
                    while (dr.Read())
                    {
                        if (dr["userTypeId"] != DBNull.Value) if (!typesList.Contains(Convert.ToInt32(dr["userTypeId"]))) typesList.Add(Convert.ToInt32(dr["userTypeId"]));
                        if (dr["permissionByUser"] != DBNull.Value) if (!permissionsList.Contains(Convert.ToInt32(dr["permissionByUser"]))) permissionsList.Add(Convert.ToInt32(dr["permissionByUser"]));
                        if (dr["permissionByUserType"] != DBNull.Value) if (!permissionsList.Contains(Convert.ToInt32(dr["permissionByUserType"]))) permissionsList.Add(Convert.ToInt32(dr["permissionByUserType"]));
                    }
                    dr.Close();
                    user.UserTypes = typesList;
                    user.Permissions = permissionsList;
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool LockUser(string UserName)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"UPDATE tblUsers SET userActive = CAST(0 AS bit) WHERE userName = '{UserName}'";
                SqlCommand cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool UnLockUser(string UserName)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                if (UserName == "SYSTEM") throw new Exception("UserNotFound");
                string str = $"UPDATE tblUsers SET userActive = CAST(1 AS bit) WHERE userName = '{UserName}'";
                SqlCommand cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool PassZero(Users user)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"UPDATE tblUsers SET userPassword = '{user.Password}', userIsNewPassword = CAST(1 AS bit) WHERE userName = '{user.UserName}'";
                SqlCommand cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<PermissionTypes> GetAllPermissionTypes()
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"SELECT * FROM tblPermissionTypes";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                List<PermissionTypes> result = new List<PermissionTypes>();
                while (dr.Read()) { result.Add(new PermissionTypes(Convert.ToInt32(dr["permissionTypeId"]), Convert.ToString(dr["permissionTypeName"]))); }
                dr.Close();
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<UserTypes> GetAllUserTypes()
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"SELECT * FROM tblUserTypes";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                List<UserTypes> result = new List<UserTypes>();
                while (dr.Read()) { result.Add(new UserTypes(Convert.ToInt32(dr["userTypeId"]), Convert.ToString(dr["userTypeName"]), new List<PermissionTypes>())); }
                dr.Close();
                foreach (UserTypes item in result)
                {
                    str = $"SELECT a.permissionTypeId as id, permissionTypeName as name " +
                        $"FROM tblUserTypesVsPermissionTypes as a left join tblPermissionTypes as b on a.permissionTypeId=b.permissionTypeId where userTypeId = {item.Id}";
                    cmd = CreateCommand(str, con);
                    dr = cmd.ExecuteReader();
                    while (dr.Read()) { item.PermissionTypes.Add(new PermissionTypes(Convert.ToInt32(dr["id"]), Convert.ToString(dr["name"]))); }
                    dr.Close();
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool RoolPost(Users user)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"select * from tblUsersVsUserTypes where userName = '{user.UserName}'";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<int> typesList = new List<int>();
                while (dr.Read()) { typesList.Add(Convert.ToInt32(dr["userTypeId"])); }
                dr.Close();
                str = $"select * from tblUsersVsPermissionTypes where userName = '{user.UserName}'";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                List<int> permissionsList = new List<int>();
                while (dr.Read()) { permissionsList.Add(Convert.ToInt32(dr["permissionTypeId"])); }
                dr.Close();
                str = "";
                for (int i = 0; i < user.UserTypes.Count; i++) if (!typesList.Contains(user.UserTypes[i])) str += $"INSERT INTO tblUsersVsUserTypes (userName, userTypeId) Values('{user.UserName}', {user.UserTypes[i]} )\n";
                for (int i = 0; i < typesList.Count; i++) if (!user.UserTypes.Contains(typesList[i])) str += $"DELETE FROM tblUsersVsUserTypes WHERE userName='{user.UserName}' AND userTypeId={typesList[i]}\n";
                for (int i = 0; i < user.Permissions.Count; i++) if (!permissionsList.Contains(user.Permissions[i])) str += $"INSERT INTO tblUsersVsPermissionTypes" +
                            $" (userName, permissionTypeId) Values('{user.UserName}', {user.Permissions[i]} )\n";
                for (int i = 0; i < permissionsList.Count; i++) if (!user.Permissions.Contains(permissionsList[i])) str += $"DELETE FROM tblUsersVsPermissionTypes" +
                            $" WHERE userName='{user.UserName}' AND permissionTypeId={permissionsList[i]}\n";
                if (str=="") throw new Exception("BadRequest");
                str = $"BEGIN TRANSACTION\n{str}IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<int> GetAllPeople()
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"select * from tblPeople";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<int> result = new List<int>();
                while (dr.Read()) { result.Add(Convert.ToInt32(dr["personId"])); }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Users AddUser(Users user)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"select * from tblUsers where userName = '{user.UserName.ToUpper()}'";
                SqlCommand cmd = CreateCommand(str, con);
                if (cmd.ExecuteNonQuery() > 0) { throw new Exception("UserInSystem"); }

                str = $"BEGIN TRANSACTION\n" +
                    $"INSERT INTO tblUsers (userName, {(user.PersonId == 0 ? "" : "personId, ")}userActive, userIsNewPassword, userPassword)" +
                    $" Values('{user.UserName.ToUpper()}', {(user.PersonId == 0 ? "" : $"{user.PersonId}, ")}1, 1, '{user.Password}')\n";
                for (int i = 0; i < user.UserTypes.Count; i++) str += $"INSERT INTO tblUsersVsUserTypes (userName, userTypeId) Values('{user.UserName.ToUpper()}', {user.UserTypes[i]} )\n";
                for (int i = 0; i < user.Permissions.Count; i++) str += $"INSERT INTO tblUsersVsPermissionTypes (userName, permissionTypeId) Values('{user.UserName.ToUpper()}', {user.Permissions[i]} )\n";
                str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                cmd = CreateCommand(str, con);
                if (cmd.ExecuteNonQuery() == 0) { throw new Exception("BadRequest"); }

                str = $"select userName, tblUsers.personId as personId, userPasswordHash, userPasswordSalt, userActive, userIsNewPassword, userPassword, personFullName, personEmail, isActiveSaleMenager" +
                    $" from  tblUsers left join tblPeople on tblUsers.personId = tblPeople.personId left join tblSaleMenager" +
                    $" on tblUsers.personId = tblSaleMenager.personId where userName = '{user.UserName.ToUpper()}'";
                cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("BadRequest"); }
                dr.Read();
                Users result = new Users(dr);
                dr.Close();

                str = $"select tblUsersVsUserTypes.userTypeId, tblUserTypesVsPermissionTypes.permissionTypeId as permissionByUserType, tblUsersVsPermissionTypes.permissionTypeId as permissionByUser" +
                    $" from tblUsersVsUserTypes left join tblUserTypesVsPermissionTypes on tblUsersVsUserTypes.userTypeId = tblUserTypesVsPermissionTypes.userTypeId" +
                    $" left join tblUsersVsPermissionTypes on tblUsersVsUserTypes.userName = tblUsersVsPermissionTypes.userName where tblUsersVsUserTypes.userName = '{user.UserName.ToUpper()}'";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                List<int> typesList = new List<int>();
                List<int> permissionsList = new List<int>();
                while (dr.Read())
                {
                    if (dr["userTypeId"] != DBNull.Value) if (!typesList.Contains(Convert.ToInt32(dr["userTypeId"]))) typesList.Add(Convert.ToInt32(dr["userTypeId"]));
                    if (dr["permissionByUser"] != DBNull.Value) if (!permissionsList.Contains(Convert.ToInt32(dr["permissionByUser"]))) permissionsList.Add(Convert.ToInt32(dr["permissionByUser"]));
                    if (dr["permissionByUserType"] != DBNull.Value) if (!permissionsList.Contains(Convert.ToInt32(dr["permissionByUserType"]))) permissionsList.Add(Convert.ToInt32(dr["permissionByUserType"]));
                }
                result.UserTypes = typesList;
                result.Permissions = permissionsList;
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool ChangePassword(string UserName, string OldPassword, string NewPassword)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"SELECT * FROM tblUsers WHERE userName = '{UserName}' and userPassword = '{OldPassword}'";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) throw new Exception("WrongPassword");
                dr.Close();

                str = $"UPDATE tblUsers SET userPassword = '{NewPassword}', userIsNewPassword = 0 WHERE userName = '{UserName}'";
                cmd = CreateCommand(str, con);
                if (cmd.ExecuteNonQuery() == 0) throw new Exception("BadRequest");
                return true;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Periods> GetAllPeriods()
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"SELECT * FROM tblPeriods";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<Periods> result = new List<Periods>();
                while (dr.Read())
                {
                    result.Add(new Periods(
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["quarterP"]),
                        Convert.ToInt32(dr["dailyProduction"]),
                        Convert.ToInt32(dr["greensUsing"]),
                        Convert.ToInt32(dr["factorS"]),
                        Convert.ToInt32(dr["factorN"]),
                        Convert.ToInt32(dr["factorD"]),
                        Convert.ToInt32(dr["factorE"]),
                        Convert.ToInt32(dr["factorOF"]),
                        Convert.ToInt32(dr["factorL"]),
                        Convert.ToInt32(dr["storageCapacity"]),
                        Convert.ToInt32(dr["conteinersInventory"])
                        ));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static int AddPeriods(List<Periods> periodsToAdd)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"BEGIN TRANSACTION\n";
                for (int i = 0; i < periodsToAdd.Count; i++)
                {
                    str += $"INSERT INTO tblPeriods (yearP, quarterP, dailyProduction, greensUsing, factorS, factorN, factorD, factorE, factorOF, factorL, storageCapacity, conteinersInventory)" +
                        $" Values({periodsToAdd[i].YearP}, {periodsToAdd[i].QuarterP}, {periodsToAdd[i].DailyProduction}, {periodsToAdd[i].GreensUsing}, {periodsToAdd[i].FactorS}," +
                        $" {periodsToAdd[i].FactorN}, {periodsToAdd[i].FactorD}, {periodsToAdd[i].FactorE}, {periodsToAdd[i].FactorOF}, {periodsToAdd[i].FactorL}," +
                        $" {periodsToAdd[i].StorageCapacity}, {periodsToAdd[i].ConteinersInventory})\n";
                }
                str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                SqlCommand cmd = CreateCommand(str, con);
                //return cmd.ExecuteNonQuery() > 0;
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<string, Countries> GetCountries()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM tblCountries";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<string, Countries> result = new Dictionary<string, Countries>();
                while (dr.Read())
                {
                    result[Convert.ToString(dr["countryId"])] = new Countries(Convert.ToString(dr["countryId"]), Convert.ToString(dr["countryName"]), Convert.ToString(dr["countryMarket"]));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdateCountries(List<Countries> countriesToAdd, List<Countries> countriesToUpdate)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "";
                int numOfErrors = 0;
                int numOfGoods = 0;
                for (int i = 0; i < countriesToAdd.Count; i++)
                {
                    str = $"insert into tblCountries (countryId, countryName, countryMarket) values ('{countriesToAdd[i].CountryId}', '{countriesToAdd[i].CountryName.Replace("'", "''")}',  '{countriesToAdd[i].CountryMarket}')";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                for (int i = 0; i < countriesToUpdate.Count; i++)
                {
                    str = $"UPDATE tblCountries SET countryName = '{countriesToUpdate[i].CountryName.Replace("'", "''")}', countryMarket = '{countriesToUpdate[i].CountryMarket}' WHERE countryId = '{countriesToUpdate[i].CountryId}'";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                return new DbUpdates(numOfErrors, numOfGoods);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<int, Person> GetPeople()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM tblPeople";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<int, Person> result = new Dictionary<int, Person>();
                while (dr.Read())
                {
                    result[Convert.ToInt32(dr["personId"])] = new Person(Convert.ToInt32(dr["personId"]), Convert.ToString(dr["personFullName"]), Convert.ToString(dr["personEmail"]));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdatePeople(List<Person> ToAdd, List<Person> ToUpdate)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "";
                int numOfErrors = 0;
                int numOfGoods = 0;
                for (int i = 0; i < ToAdd.Count; i++)
                {
                    str = $"insert into tblPeople (personId, personFullName, personEmail) values ({ToAdd[i].Id}, '{ToAdd[i].FullName.Replace("'", "''")}',  '{ToAdd[i].Email}')";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                for (int i = 0; i < ToUpdate.Count; i++)
                {
                    str = $"UPDATE tblPeople SET personFullName = '{ToUpdate[i].FullName.Replace("'", "''")}', personEmail = '{ToUpdate[i].Email}' WHERE personId = '{ToUpdate[i].Id}'";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                return new DbUpdates(numOfErrors, numOfGoods);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<int, SaleMenagers> GetSaleMenagers()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM tblSaleMenager";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<int, SaleMenagers> result = new Dictionary<int, SaleMenagers>();
                while (dr.Read())
                {
                    result[Convert.ToInt32(dr["personId"])] = new SaleMenagers(Convert.ToInt32(dr["personId"]), Convert.ToBoolean(dr["isActiveSaleMenager"]));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdateSaleMenagers(List<SaleMenagers> ToAdd, List<SaleMenagers> ToUpdate)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "";
                int numOfErrors = 0;
                int numOfGoods = 0;
                for (int i = 0; i < ToAdd.Count; i++)
                {
                    str = $"insert into tblSaleMenager (personId, isActiveSaleMenager) values ({ToAdd[i].Id}, {(ToAdd[i].Active ? "1" : "0")})";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                for (int i = 0; i < ToUpdate.Count; i++)
                {
                    str = $"UPDATE tblSaleMenager SET isActiveSaleMenager = {(ToUpdate[i].Active ? "1" : "0")} WHERE personId = '{ToUpdate[i].Id}'";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                return new DbUpdates(numOfErrors, numOfGoods);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<int, Customers> GetCustomers()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT Cust.*, sh.countryId FROM tblCustomers as Cust left join tblShipTos as Sh on Cust.customerNumber=sh.customerNumber where sh.shipToIsEndCustomer=0";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<int, Customers> result = new Dictionary<int, Customers>();
                while (dr.Read())
                {
                    result[Convert.ToInt32(dr["customerNumber"])] = new Customers(Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToBoolean(dr["isLocal"]),
                        Convert.ToInt32(dr["saleMenagerId"]),
                        Convert.ToString(dr["countryId"])
                        );
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Customers GetCustomer(int customerNumber)
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT Cust.*, sh.countryId FROM tblCustomers as Cust left join tblShipTos as Sh on Cust.customerNumber=sh.customerNumber where sh.shipToIsEndCustomer=0 and Cust.customerNumber={customerNumber}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("CustomerNotFound"); }
                dr.Read();
                return new Customers(Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToBoolean(dr["isLocal"]),
                        Convert.ToInt32(dr["saleMenagerId"]),
                        Convert.ToString(dr["countryId"])
                        );
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdateCustomers(List<Customers> ToAdd, List<Customers> ToUpdate)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "";
                int numOfErrors = 0;
                int numOfGoods = 0;
                for (int i = 0; i < ToAdd.Count; i++)
                {
                    str = $"BEGIN TRANSACTION\n";
                    str += $"insert into tblCustomers (customerNumber, customerName, isLocal, saleMenagerId) values ({ToAdd[i].Number}, '{ToAdd[i].Name}', {(ToAdd[i].IsLocal ? "1" : "0")}, {ToAdd[i].SaleMenagerId})\n";
                    str += $"insert into tblShipTos (customerNumber, shipToName, countryId, shipToActive, shipToIsEndCustomer) values ({ToAdd[i].Number}, '', '{ToAdd[i].Country.CountryId}', 1, 0)\n";
                    str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                for (int i = 0; i < ToUpdate.Count; i++)
                {
                    str = $"BEGIN TRANSACTION\n";
                    str += $"UPDATE tblCustomers SET customerName = '{ToUpdate[i].Name}', isLocal = {(ToUpdate[i].IsLocal ? "1" : "0")}, saleMenagerId = {ToUpdate[i].SaleMenagerId} WHERE customerNumber = {ToUpdate[i].Number}\n";
                    str += $"UPDATE tblShipTos SET countryId = '{ToUpdate[i].Country.CountryId}' WHERE customerNumber = {ToUpdate[i].Number} AND shipToIsEndCustomer = 0\n";
                    str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                return new DbUpdates(numOfErrors, numOfGoods);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<string, Items> GetItems()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM tblItems";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<string, Items> result = new Dictionary<string, Items>();
                while (dr.Read())
                {
                    result[Convert.ToString(dr["itemNumber"])] = new Items(Convert.ToString(dr["itemNumber"]), Convert.ToString(dr["itemDescription"]), Convert.ToString(dr["productFamilyId"]));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdateItems(List<Items> ToAdd, List<Items> ToUpdate)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "";
                int numOfErrors = 0;
                int numOfGoods = 0;
                for (int i = 0; i < ToAdd.Count; i++)
                {
                    str = $"insert into tblItems (itemNumber, itemDescription, ProductFamilyId) values ('{ToAdd[i].Number}', '{ToAdd[i].Description}', '{ToAdd[i].ProductFamilyId}')";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                for (int i = 0; i < ToUpdate.Count; i++)
                {
                    str = $"UPDATE tblItems SET itemDescription = '{ToUpdate[i].Description}', ProductFamilyId = '{ToUpdate[i].ProductFamilyId}' WHERE itemNumber = '{ToUpdate[i].Number}'";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                return new DbUpdates(numOfErrors, numOfGoods);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        //public static List<SaleRows> GetSaleRows()
        //{
        //    SqlConnection con = connect();

        //    try
        //    {
        //        string str = $"SELECT * FROM tblSales";
        //        SqlCommand cmd = CreateCommand(str, con);
        //        SqlDataReader dr = cmd.ExecuteReader();
        //        List<SaleRows> result = new List<SaleRows>();
        //        while (dr.Read())
        //        {
        //            result.Add(new SaleRows(Convert.ToInt32(dr["yearP"]), Convert.ToInt32(dr["quarterP"]), Convert.ToString(dr["itemNumber"]), Convert.ToInt32(dr["customerNumber"]), Convert.ToDouble(dr["salesQty"])));
        //        }
        //        return result;
        //    }
        //    catch (Exception ex) { throw ex; }
        //    finally { if (con != null) con.Close(); }
        //}

        //public static DbUpdates AddUpdateSaleRows(List<SaleRows> ToAdd, List<SaleRows> ToUpdate)
        //{
        //    SqlConnection con = connect("DBConnectionString");

        //    try
        //    {
        //        SqlCommand cmd;
        //        string str = "";
        //        int numOfErrors = 0;
        //        int numOfGoods = 0;
        //        for (int i = 0; i < ToAdd.Count; i++)
        //        {
        //            str = $"insert into tblSales (itemNumber, yearP, quarterP, customerNumber, salesQty)" +
        //                $" values ('{ToAdd[i].ItemNumber}', {ToAdd[i].Year}, {ToAdd[i].Quarter}, {ToAdd[i].CustomerNumber}, {ToAdd[i].Qty.ToString().Replace(",", ".")})";
        //            cmd = CreateCommand(str, con);
        //            try
        //            {
        //                if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
        //                else numOfErrors++;
        //            }
        //            catch { numOfErrors++; }
        //        }
        //        for (int i = 0; i < ToUpdate.Count; i++)
        //        {
        //            str = $"UPDATE tblSales SET salesQty = '{ToUpdate[i].Qty.ToString().Replace(",", ".")}'" +
        //                $" WHERE itemNumber = '{ToUpdate[i].ItemNumber}' AND yearP = {ToUpdate[i].Year} AND yearP = {ToUpdate[i].Quarter} AND yearP = {ToUpdate[i].CustomerNumber}";
        //            cmd = CreateCommand(str, con);
        //            try
        //            {
        //                if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
        //                else numOfErrors++;
        //            }
        //            catch { numOfErrors++; }
        //        }
        //        return new DbUpdates(numOfErrors, numOfGoods);
        //    }
        //    catch (Exception ex) { throw ex; }
        //    finally { if (con != null) con.Close(); }
        //}

        public static int GetMaxSalePeriod()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"select isnull(max((yearP - 2013) * 4 + quarterP), 0) as maxPeriod from tblSales";
                SqlCommand cmd = CreateCommand(str, con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdateSaleRows(List<SaleRows> ToAdd)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "BEGIN TRANSACTION\n";
                for (int i = 0; i < ToAdd.Count; i++)
                {
                    str += $"insert into tblSales (itemNumber, yearP, quarterP, customerNumber, salesQty)" +
                        $" values ('{ToAdd[i].ItemNumber}', {ToAdd[i].Year}, {ToAdd[i].Quarter}, {ToAdd[i].CustomerNumber}, {ToAdd[i].Qty.ToString().Replace(",", ".")})\n";
                }
                str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                cmd = CreateCommand(str, con);
                return new DbUpdates(0, cmd.ExecuteNonQuery());
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<ShipTos> GetShipTos()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT Sh.* , countryName, countryMarket, customerName" +
                    $" FROM tblShipTos as Sh left join tblCountries as Co on Sh.countryId = Co.countryId left join tblCustomers as Cu on Sh.customerNumber = Cu.customerNumber" +
                    $" WHERE shipToIsEndCustomer = 1";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                List<ShipTos> result = new List<ShipTos>();
                while (dr.Read())
                {
                    result.Add(new ShipTos(
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["shipToName"]),
                        Convert.ToString(dr["countryId"]),
                        Convert.ToString(dr["countryName"]),
                        Convert.ToBoolean(dr["shipToActive"]),
                        Convert.ToBoolean(dr["shipToIsEndCustomer"])
                        ));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool UpdateShipToActive(ShipTos ShipTo)
        {
            SqlConnection con = connect();

            try
            {
                string str = $"select * from tblShipTos where customerNumber = {ShipTo.CustomerNumber} and shipToName = '{ShipTo.ShipToName}'";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                dr.Close();
                str = $"UPDATE tblShipTos SET shipToActive = {(ShipTo.Active ? "1" : "0")} WHERE customerNumber = {ShipTo.CustomerNumber} and shipToName = '{ShipTo.ShipToName}'";
                cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static ShipTos AddShipTo(ShipTos ShipTo)
        {
            SqlConnection con = connect();

            try
            {
                string str = $"select * from tblShipTos where customerNumber = {ShipTo.CustomerNumber} and shipToName = '{ShipTo.ShipToName}'";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows) { throw new Exception("Exists"); }
                dr.Close();
                str = $"select * from tblCustomers where customerNumber = {ShipTo.CustomerNumber}";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("CustomerNotFound"); }
                dr.Close();
                str = $"select * from tblCountries where countryId = '{ShipTo.CountryId}'";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("CountryNotFound"); }
                dr.Close();
                str = $"insert into tblShipTos (customerNumber, shipToName, countryId, shipToActive, shipToIsEndCustomer)" +
                    $" values ({ShipTo.CustomerNumber}, '{ShipTo.ShipToName}', '{ShipTo.CountryId}', 1, 1)";
                cmd = CreateCommand(str, con);
                if (cmd.ExecuteNonQuery() == 0) throw new Exception("ServerError");
                str = $"SELECT Sh.* , countryName, countryMarket, customerName" +
                    $" FROM tblShipTos as Sh left join tblCountries as Co on Sh.countryId = Co.countryId left join tblCustomers as Cu on Sh.customerNumber = Cu.customerNumber" +
                    $" WHERE Sh.customerNumber = {ShipTo.CustomerNumber} and Sh.shipToName = '{ShipTo.ShipToName}'";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                dr.Read();
                return new ShipTos(
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["shipToName"]),
                        Convert.ToString(dr["countryId"]),
                        Convert.ToString(dr["countryName"]),
                        Convert.ToBoolean(dr["shipToActive"]),
                        Convert.ToBoolean(dr["shipToIsEndCustomer"])
                        );
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static int GetMaxAbsWeekNumber()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"select isnull(max(yearP * 100 + weekNumber), (select min(yearP * 100) from tblForecastTransactions)) as YearWeek from tblWeeks";
                SqlCommand cmd = CreateCommand(str, con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddWeeks(List<Weeks> ToAdd)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                SqlCommand cmd;
                string str = "";
                int numOfErrors = 0;
                int numOfGoods = 0;
                for (int i = 0; i < ToAdd.Count; i++)
                {
                    str = $"insert into tblWeeks (yearP, quarterP, weekNumber, actualProduction, actualGreens, actualSales, actualInventory, actualInventoryS, actualInventoryN," +
                        $" actualInventoryD, actualInventoryE, actualInventoryOF, actualInventoryExternal)" +
                        $" values ({ToAdd[i].YearP}, {ToAdd[i].QuarterP}, {ToAdd[i].WeekNumber}, {ToAdd[i].ActualProduction.ToString().Replace(",", ".")}, {ToAdd[i].ActualGreens.ToString().Replace(",", ".")}," +
                        $" {ToAdd[i].ActualSales.ToString().Replace(",", ".")}, {ToAdd[i].ActualInventory.ToString().Replace(",", ".")}, {ToAdd[i].ActualInventoryS.ToString().Replace(",", ".")}," +
                        $" {ToAdd[i].ActualInventoryN.ToString().Replace(",", ".")}, {ToAdd[i].ActualInventoryD.ToString().Replace(",", ".")}, {ToAdd[i].ActualInventoryE.ToString().Replace(",", ".")}," +
                        $" {ToAdd[i].ActualInventoryOF.ToString().Replace(",", ".")}, {ToAdd[i].ActualInventoryExternal.ToString().Replace(",", ".")})";
                    cmd = CreateCommand(str, con);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0) numOfGoods++;
                        else numOfErrors++;
                    }
                    catch { numOfErrors++; }
                }
                return new DbUpdates(numOfErrors, numOfGoods);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<ForecastRows> GetForecast(int year, string fDate = null, Users user = null)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                //Completed
                string str = $"WITH tbl AS (SELECT * FROM tblForecastTransactions WHERE forecastTransactionStatus = 'Completed'" +
                    $" AND yearP = {year}{(fDate is null ? "" : $" AND forecastTransactionCreationDate<='{fDate}'")})\n" +
                    $"SELECT forc.* ,countryName, countryMarket, itemDescription, productFamilyName, customerName, personFullName" +
                    $" FROM(SELECT t1.itemNumber, t1.yearP, t1.customerNumber, t1.shipToName, t1.forecastTransactionIsMelting," +
                    $" SUM(CASE WHEN(t1.quarterP = 1) THEN t1.forecastTransactionQty ELSE 0 END) AS Q1," +
                    $" SUM(CASE WHEN(t1.quarterP = 2) THEN t1.forecastTransactionQty ELSE 0 END) AS Q2," +
                    $" SUM(CASE WHEN(t1.quarterP = 3) THEN t1.forecastTransactionQty ELSE 0 END) AS Q3," +
                    $" SUM(CASE WHEN(t1.quarterP = 4) THEN t1.forecastTransactionQty ELSE 0 END) AS Q4" +
                    $" FROM tbl t1 LEFT JOIN tbl t2" +
                    $" ON(t1.itemNumber = t2.itemNumber AND t1.yearP = t2.yearP AND t1.quarterP = t2.quarterP AND t1.customerNumber = t2.customerNumber" +
                    $" AND t1.shipToName = t2.shipToName AND t1.forecastTransactionCreationDate < t2.forecastTransactionCreationDate) LEFT JOIN tbl t3" +
                    $" ON(t1.itemNumber = t3.itemNumber AND t1.yearP = t3.yearP AND t1.quarterP = t3.quarterP AND t1.customerNumber = t3.customerNumber" +
                    $" AND t1.shipToName = t3.shipToName AND t1.forecastTransactionsId < t3.forecastTransactionsId)" +
                    $" WHERE t2.forecastTransactionCreationDate IS NULL AND t3.forecastTransactionsId IS NULL" +
                    $" GROUP BY t1.itemNumber, t1.yearP, t1.customerNumber, t1.shipToName, t1.forecastTransactionIsMelting) AS forc" +
                    $" LEFT JOIN tblShipTos ON(forc.customerNumber= tblShipTos.customerNumber AND forc.shipToName= tblShipTos.shipToName)" +
                    $" LEFT JOIN tblCountries ON tblShipTos.countryId = tblCountries.countryId" +
                    $" LEFT JOIN tblItems ON forc.itemNumber = tblItems.itemNumber" +
                    $" LEFT JOIN tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" LEFT JOIN tblCustomers ON forc.customerNumber = tblCustomers.customerNumber" +
                    $" LEFT JOIN tblPeople ON tblCustomers.saleMenagerId = tblPeople.personId" +
                    $"{(user is null ? "" : (user.Permissions.Contains(1) ? "" : (user.Permissions.Contains(2) ? " WHERE isLocal = 1" : (user.IsSaleMenager ? $" WHERE saleMenagerId = {user.PersonId}" : ""))))}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                List<ForecastRows> result = new List<ForecastRows>();
                while (dr.Read())
                {
                    result.Add(new ForecastRows(Convert.ToString(dr["itemNumber"]),
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["shipToName"]),
                        Convert.ToBoolean(dr["forecastTransactionIsMelting"]) ? "Melting" : "Classic",
                        Convert.ToDouble(dr["Q1"]),
                        Convert.ToDouble(dr["Q2"]),
                        Convert.ToDouble(dr["Q3"]),
                        Convert.ToDouble(dr["Q4"]),
                        Convert.ToString(dr["countryName"]),
                        Convert.ToString(dr["countryMarket"]),
                        Convert.ToString(dr["itemDescription"]),
                        Convert.ToString(dr["productFamilyName"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["personFullName"])));
                }
                dr.Close();
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<int> GetForecastYears()
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string str = $"select distinct yearP from tblForecastTransactions order by yearP";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                List<int> result = new List<int>();
                while (dr.Read()) { result.Add(Convert.ToInt32(dr["yearP"])); }
                dr.Close();
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool UpdateForecast(Users user, List<ForecastRows> forecastToUpdate)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string str = "BEGIN TRANSACTION\n";
                foreach (ForecastRows row in forecastToUpdate) foreach (int q in row.QuartersToUpdate)
                    {
                        str += $"UPDATE tblForecastTransactions SET forecastTransactionStatus = 'Canceled', forecastTransactionLastUpdateDate = '{nowStr}', forecastTransactionLastUpdateBy = 'SYSTEM'" +
                            $" WHERE itemNumber = '{row.ItemNumber}' AND yearP = {row.Year} AND quarterP = {q} AND customerNumber = {row.CustomerNumber}" +
                            $" AND shipToName = '{row.ShipToName}' AND forecastTransactionStatus = 'Pending'\n";
                        str += $"insert into tblForecastTransactions (itemNumber, yearP, quarterP, customerNumber, shipToName, forecastTransactionCreatedBy, forecastTransactionLastUpdateBy," +
                            $" forecastTransactionCreationDate, forecastTransactionQty, forecastTransactionStatus, forecastTransactionLastUpdateDate, forecastTransactionIsMelting)" +
                            $" values ('{row.ItemNumber}', {row.Year}, {q}, {row.CustomerNumber}, '{row.ShipToName}', '{user.UserName}', '{user.UserName}', '{nowStr}'," +
                            $" {row.QValue(q).ToString().Replace(",", ".")}, {(user.Permissions.Contains(1) || user.Permissions.Contains(2) ? "'Completed'" : "'Pending'")}," +
                            $" '{nowStr}', {(row.MeltingClassic == "Melting" ? "1" : "0")})\n";
                    }
                str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                SqlCommand cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static ForecastRows AddNewForecastRow(Users user, ForecastRows row, Customers customer)
        {
            SqlConnection con = connect("DBConnectionString");

            try
            {
                string nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string str = $"select * from tblForecastTransactions where itemNumber = '{row.ItemNumber}' AND yearP = {row.Year} AND customerNumber = {row.CustomerNumber}" +
                    $" AND shipToName = '{row.ShipToName}' AND (forecastTransactionStatus = 'Completed' OR forecastTransactionStatus = 'Pending')";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows) { throw new Exception("Exists"); }
                dr.Close();
                str = "BEGIN TRANSACTION\n";
                for (int i = 1; i <= 4; i++)
                {
                    str += $"insert into tblForecastTransactions (itemNumber, yearP, quarterP, customerNumber, shipToName, forecastTransactionCreatedBy, forecastTransactionLastUpdateBy," +
                            $" forecastTransactionCreationDate, forecastTransactionQty, forecastTransactionStatus, forecastTransactionLastUpdateDate, forecastTransactionIsMelting)" +
                            $" values ('{row.ItemNumber}', {row.Year}, {i}, {row.CustomerNumber}, '{row.ShipToName}', '{user.UserName}', '{user.UserName}', '{nowStr}'," +
                            $" {row.QValue(i).ToString().Replace(",", ".")}, {(user.Permissions.Contains(1) || (user.Permissions.Contains(2) && customer.IsLocal) ? "'Completed'" : "'Pending'")}," +
                            $" '{nowStr}', {(row.MeltingClassic == "Melting" ? "1" : "0")})\n";
                }
                str += "IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                cmd = CreateCommand(str, con);
                if (cmd.ExecuteNonQuery() == 0) { throw new Exception("BadRequest"); }
                if (!user.Permissions.Contains(1) && !(user.Permissions.Contains(2) && customer.IsLocal)) { throw new Exception("Pending"); }
                str = $"WITH tbl AS (SELECT * FROM tblForecastTransactions WHERE itemNumber = '{row.ItemNumber}' AND yearP = {row.Year} AND customerNumber = {row.CustomerNumber}" +
                    $" AND shipToName = '{row.ShipToName}' AND forecastTransactionStatus = 'Completed')\n" +
                    $"SELECT forc.* ,countryName, countryMarket, itemDescription, productFamilyName, customerName, personFullName" +
                    $" FROM(SELECT t1.itemNumber, t1.yearP, t1.customerNumber, t1.shipToName, t1.forecastTransactionIsMelting," +
                    $" SUM(CASE WHEN(t1.quarterP = 1) THEN t1.forecastTransactionQty ELSE 0 END) AS Q1," +
                    $" SUM(CASE WHEN(t1.quarterP = 2) THEN t1.forecastTransactionQty ELSE 0 END) AS Q2," +
                    $" SUM(CASE WHEN(t1.quarterP = 3) THEN t1.forecastTransactionQty ELSE 0 END) AS Q3," +
                    $" SUM(CASE WHEN(t1.quarterP = 4) THEN t1.forecastTransactionQty ELSE 0 END) AS Q4" +
                    $" FROM tbl t1 LEFT JOIN tbl t2" +
                    $" ON(t1.itemNumber = t2.itemNumber AND t1.yearP = t2.yearP AND t1.quarterP = t2.quarterP AND t1.customerNumber = t2.customerNumber" +
                    $" AND t1.shipToName = t2.shipToName AND t1.forecastTransactionCreationDate < t2.forecastTransactionCreationDate) LEFT JOIN tbl t3" +
                    $" ON(t1.itemNumber = t3.itemNumber AND t1.yearP = t3.yearP AND t1.quarterP = t3.quarterP AND t1.customerNumber = t3.customerNumber" +
                    $" AND t1.shipToName = t3.shipToName AND t1.forecastTransactionsId < t3.forecastTransactionsId)" +
                    $" WHERE t2.forecastTransactionCreationDate IS NULL AND t3.forecastTransactionsId IS NULL" +
                    $" GROUP BY t1.itemNumber, t1.yearP, t1.customerNumber, t1.shipToName, t1.forecastTransactionIsMelting) AS forc" +
                    $" LEFT JOIN tblShipTos ON(forc.customerNumber= tblShipTos.customerNumber AND forc.shipToName= tblShipTos.shipToName)" +
                    $" LEFT JOIN tblCountries ON tblShipTos.countryId = tblCountries.countryId" +
                    $" LEFT JOIN tblItems ON forc.itemNumber = tblItems.itemNumber" +
                    $" LEFT JOIN tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" LEFT JOIN tblCustomers ON forc.customerNumber = tblCustomers.customerNumber" +
                    $" LEFT JOIN tblPeople ON tblCustomers.saleMenagerId = tblPeople.personId";
                cmd = CreateCommand(str, con);
                dr = cmd.ExecuteReader();
                dr.Read();
                return new ForecastRows(Convert.ToString(dr["itemNumber"]),
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["shipToName"]),
                        Convert.ToBoolean(dr["forecastTransactionIsMelting"]) ? "Melting" : "Classic",
                        Convert.ToDouble(dr["Q1"]),
                        Convert.ToDouble(dr["Q2"]),
                        Convert.ToDouble(dr["Q3"]),
                        Convert.ToDouble(dr["Q4"]),
                        Convert.ToString(dr["countryName"]),
                        Convert.ToString(dr["countryMarket"]),
                        Convert.ToString(dr["itemDescription"]),
                        Convert.ToString(dr["productFamilyName"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["personFullName"]));
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }
    }
}