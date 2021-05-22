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
        private static SqlConnection Connect(string conString = "DBConnectionString")
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
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = CommandSTR,
                CommandTimeout = timeout,
                CommandType = System.Data.CommandType.Text
            };
            return cmd;
        }

        public static Users LogIn(string UserName, string Password)
        {
            SqlConnection con = Connect("DBConnectionString");
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
            SqlConnection con = Connect("DBConnectionString");
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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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

        public static List<Periods> GetAllPeriods(int year = 0)
        {
            SqlConnection con = Connect("DBConnectionString");
            string whereStr = "";
            if (year != 0) whereStr = $" WHERE yearP = {year}";

            try
            {
                string str = $"SELECT * FROM tblPeriods{whereStr}";
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
            SqlConnection con = Connect("DBConnectionString");

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
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<string, Countries> GetCountries()
        {
            SqlConnection con = Connect();

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect("DBConnectionString");

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

        public static int GetMaxSalePeriod()
        {
            SqlConnection con = Connect();

            try
            {
                string str = $"select isnull(max((yearP - {Periods.FirstYear}) * 4 + quarterP), 0) as maxPeriod from tblSales";
                SqlCommand cmd = CreateCommand(str, con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static DbUpdates AddUpdateSaleRows(List<SaleRows> ToAdd)
        {
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect();

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

            try
            {
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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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
            SqlConnection con = Connect("DBConnectionString");

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

        public static int GetNumOfApprovals(Users user)
        {
            SqlConnection con = Connect("DBConnectionString");
            string whereStr = $"{(user.Permissions.Contains(3) ? "" : $"{(user.Permissions.Contains(4) ? " and isLocal=1" : $" and personId={user.PersonId}")}")}";

            try
            {
                string str = $"select count(*) as num from tblForecastTransactions as forc" +
                    $" left join tblShipTos ON(forc.customerNumber= tblShipTos.customerNumber AND forc.shipToName= tblShipTos.shipToName)" +
                    $" left join tblCountries ON tblShipTos.countryId = tblCountries.countryId" +
                    $" left join tblItems ON forc.itemNumber = tblItems.itemNumber" +
                    $" left join tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" left join tblCustomers ON forc.customerNumber = tblCustomers.customerNumber" +
                    $" left join tblPeople ON tblCustomers.saleMenagerId = tblPeople.personId" +
                    $" where forecastTransactionStatus = 'Pending'{whereStr}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                int result = Convert.ToInt32(dr["num"]);
                dr.Close();
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<ApprovalRows> GetApprovals(Users user)
        {
            SqlConnection con = Connect("DBConnectionString");
            string whereStr = $"{(user.Permissions.Contains(3) ? "" : $"{(user.Permissions.Contains(4) ? " and isLocal=1" : $" and personId={user.PersonId}")}")}";

            try
            {
                string str = $"select forc.* ,countryName, countryMarket, itemDescription, productFamilyName, customerName, personFullName" +
                    $" from tblForecastTransactions as forc" +
                    $" left join tblShipTos ON(forc.customerNumber= tblShipTos.customerNumber AND forc.shipToName= tblShipTos.shipToName)" +
                    $" left join tblCountries ON tblShipTos.countryId = tblCountries.countryId" +
                    $" left join tblItems ON forc.itemNumber = tblItems.itemNumber" +
                    $" left join tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" left join tblCustomers ON forc.customerNumber = tblCustomers.customerNumber" +
                    $" left join tblPeople ON tblCustomers.saleMenagerId = tblPeople.personId" +
                    $" where forecastTransactionStatus = 'Pending'{whereStr}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<ApprovalRows> result = new List<ApprovalRows>();
                while (dr.Read())
                {
                    result.Add(new ApprovalRows(
                        Convert.ToDateTime(dr["forecastTransactionCreationDate"]),
                        Convert.ToString(dr["itemNumber"]),
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["quarterP"]),
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["shipToName"]),
                        Convert.ToDouble(dr["forecastTransactionQty"]),
                        Convert.ToString(dr["countryName"]),
                        Convert.ToString(dr["countryMarket"]),
                        Convert.ToString(dr["itemDescription"]),
                        Convert.ToString(dr["productFamilyName"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["personFullName"]),
                        Convert.ToInt32(dr["forecastTransactionsId"])
                        ));
                }
                dr.Close();
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool PostApprovals(Users user, int id, string action)
        {
            SqlConnection con = Connect("DBConnectionString");

            try
            {
                string str = $"select forc.* ,countryName, countryMarket, itemDescription, productFamilyName, customerName, personFullName, isLocal, personId" +
                    $" from tblForecastTransactions as forc" +
                    $" left join tblShipTos ON(forc.customerNumber= tblShipTos.customerNumber AND forc.shipToName= tblShipTos.shipToName)" +
                    $" left join tblCountries ON tblShipTos.countryId = tblCountries.countryId" +
                    $" left join tblItems ON forc.itemNumber = tblItems.itemNumber" +
                    $" left join tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" left join tblCustomers ON forc.customerNumber = tblCustomers.customerNumber" +
                    $" left join tblPeople ON tblCustomers.saleMenagerId = tblPeople.personId" +
                    $" where forecastTransactionsId={id}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                dr.Read();
                bool isLocal = Convert.ToBoolean(dr["isLocal"]);
                if (!user.Permissions.Contains(3)) if (user.Permissions.Contains(4) && !isLocal)
                    {
                        if (!user.IsSaleMenager) throw new Exception("Forbidden");
                        else if (user.PersonId != Convert.ToInt32(dr["personId"])) throw new Exception("Forbidden");
                    }
                string status = Convert.ToString(dr["forecastTransactionStatus"]);
                if (status == "Completed") throw new Exception("Completed");
                if (status == "Canceled") throw new Exception("Canceled");
                dr.Close();
                str = $"UPDATE tblForecastTransactions SET forecastTransactionStatus = '{(action == "allow" ? "Completed" : "Canceled")}', forecastTransactionLastUpdateBy = '{user.UserName}'" +
                    $", forecastTransactionLastUpdateDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE forecastTransactionsId = {id}";
                cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<StatisticalForecastRow> GetStatisticalForecastRows(int year)
        {
            SqlConnection con = Connect("DBConnectionString");

            try
            {
                string str = $"select yearP, customerNumber, tblProductFamilies.productFamilyId," +
                    $" SUM(CASE WHEN(quarterP = 1) THEN salesQty ELSE 0 END) AS Q1Sales," +
                    $" SUM(CASE WHEN(quarterP = 2) THEN salesQty ELSE 0 END) AS Q2Sales," +
                    $" SUM(CASE WHEN(quarterP = 3) THEN salesQty ELSE 0 END) AS Q3Sales," +
                    $" SUM(CASE WHEN(quarterP = 4) THEN salesQty ELSE 0 END) AS Q4Sales" +
                    $" into #TS from tblSales" +
                    $" left join tblItems on tblSales.itemNumber = tblItems.itemNumber" +
                    $" left join tblProductFamilies on tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" group by yearP, customerNumber, tblProductFamilies.productFamilyId;" +
                    $"WITH tbl AS(SELECT * FROM tblForecastTransactions WHERE forecastTransactionStatus = 'Completed' and yearP = {year})" +
                    $" SELECT forc.* , customerName, personFullName," +
                    $" TS1.Q1Sales as Q11, TS1.Q2Sales as Q21, TS1.Q3Sales as Q31, TS1.Q4Sales as Q41," +
                    $" TS2.Q1Sales as Q12, TS1.Q2Sales as Q22, TS1.Q3Sales as Q32, TS1.Q4Sales as Q42," +
                    $" TS3.Q1Sales as Q13, TS1.Q2Sales as Q23, TS1.Q3Sales as Q33, TS1.Q4Sales as Q43," +
                    $" TS4.Q1Sales as Q14, TS1.Q2Sales as Q24, TS1.Q3Sales as Q34, TS1.Q4Sales as Q44" +
                    $" FROM(SELECT t1.yearP, t1.customerNumber, tblProductFamilies.productFamilyId, tblProductFamilies.productFamilyName," +
                    $" SUM(CASE WHEN(t1.quarterP = 1) THEN t1.forecastTransactionQty ELSE 0 END) AS Q1F," +
                    $" SUM(CASE WHEN(t1.quarterP = 2) THEN t1.forecastTransactionQty ELSE 0 END) AS Q2F," +
                    $" SUM(CASE WHEN(t1.quarterP = 3) THEN t1.forecastTransactionQty ELSE 0 END) AS Q3F," +
                    $" SUM(CASE WHEN(t1.quarterP = 4) THEN t1.forecastTransactionQty ELSE 0 END) AS Q4F" +
                    $" FROM tbl t1 LEFT JOIN tbl t2" +
                    $" ON(t1.itemNumber = t2.itemNumber AND t1.yearP = t2.yearP AND t1.quarterP = t2.quarterP AND t1.customerNumber = t2.customerNumber" +
                    $" AND t1.shipToName = t2.shipToName AND t1.forecastTransactionCreationDate < t2.forecastTransactionCreationDate) LEFT JOIN tbl t3" +
                    $" ON(t1.itemNumber = t3.itemNumber AND t1.yearP = t3.yearP AND t1.quarterP = t3.quarterP AND t1.customerNumber = t3.customerNumber" +
                    $" AND t1.shipToName = t3.shipToName AND t1.forecastTransactionsId < t3.forecastTransactionsId)" +
                    $" LEFT JOIN tblItems ON t1.itemNumber = tblItems.itemNumber" +
                    $" LEFT JOIN tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" WHERE t2.forecastTransactionCreationDate IS NULL AND t3.forecastTransactionsId IS NULL" +
                    $" GROUP BY tblProductFamilies.productFamilyId, t1.yearP, t1.customerNumber, tblProductFamilies.productFamilyName) AS forc" +
                    $" LEFT JOIN tblCustomers ON forc.customerNumber = tblCustomers.customerNumber" +
                    $" LEFT JOIN tblPeople ON tblCustomers.saleMenagerId = tblPeople.personId" +
                    $" left join #TS as TS1 on (forc.yearP - 1 = TS1.yearP and forc.customerNumber = TS1.customerNumber and forc.productFamilyId=TS1.productFamilyId)" +
                    $" left join #TS as TS2 on (forc.yearP - 2 = TS2.yearP and forc.customerNumber = TS2.customerNumber and forc.productFamilyId=TS2.productFamilyId)" +
                    $" left join #TS as TS3 on (forc.yearP - 3 = TS3.yearP and forc.customerNumber = TS3.customerNumber and forc.productFamilyId=TS3.productFamilyId)" +
                    $" left join #TS as TS4 on (forc.yearP - 4 = TS4.yearP and forc.customerNumber = TS4.customerNumber and forc.productFamilyId=TS4.productFamilyId)" +
                    $" drop table #TS";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<StatisticalForecastRow> result = new List<StatisticalForecastRow>();
                while (dr.Read())
                {
                    double q1F = dr["Q1F"] != DBNull.Value ? Convert.ToDouble(dr["Q1F"]) : 0;
                    double q2F = dr["Q2F"] != DBNull.Value ? Convert.ToDouble(dr["Q2F"]) : 0;
                    double q3F = dr["Q3F"] != DBNull.Value ? Convert.ToDouble(dr["Q3F"]) : 0;
                    double q4F = dr["Q4F"] != DBNull.Value ? Convert.ToDouble(dr["Q4F"]) : 0;
                    double q11 = dr["Q11"] != DBNull.Value ? Convert.ToDouble(dr["Q11"]) : 0;
                    double q21 = dr["Q21"] != DBNull.Value ? Convert.ToDouble(dr["Q21"]) : 0;
                    double q31 = dr["Q31"] != DBNull.Value ? Convert.ToDouble(dr["Q31"]) : 0;
                    double q41 = dr["Q41"] != DBNull.Value ? Convert.ToDouble(dr["Q41"]) : 0;
                    double q12 = dr["Q12"] != DBNull.Value ? Convert.ToDouble(dr["Q12"]) : 0;
                    double q22 = dr["Q22"] != DBNull.Value ? Convert.ToDouble(dr["Q22"]) : 0;
                    double q32 = dr["Q32"] != DBNull.Value ? Convert.ToDouble(dr["Q32"]) : 0;
                    double q42 = dr["Q42"] != DBNull.Value ? Convert.ToDouble(dr["Q42"]) : 0;
                    double q13 = dr["Q13"] != DBNull.Value ? Convert.ToDouble(dr["Q13"]) : 0;
                    double q23 = dr["Q23"] != DBNull.Value ? Convert.ToDouble(dr["Q23"]) : 0;
                    double q33 = dr["Q33"] != DBNull.Value ? Convert.ToDouble(dr["Q33"]) : 0;
                    double q43 = dr["Q43"] != DBNull.Value ? Convert.ToDouble(dr["Q43"]) : 0;
                    double q14 = dr["Q14"] != DBNull.Value ? Convert.ToDouble(dr["Q14"]) : 0;
                    double q24 = dr["Q24"] != DBNull.Value ? Convert.ToDouble(dr["Q24"]) : 0;
                    double q34 = dr["Q34"] != DBNull.Value ? Convert.ToDouble(dr["Q34"]) : 0;
                    double q44 = dr["Q44"] != DBNull.Value ? Convert.ToDouble(dr["Q44"]) : 0;
                    result.Add(new StatisticalForecastRow(
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["productFamilyName"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["personFullName"]),
                        new double[4, 5]{ { q1F, q11, q12, q13, q14 },
                            { q2F, q21, q22, q23, q24 },
                            { q3F, q31, q32, q33, q34 },
                            { q4F, q41, q42, q43, q44 } }
                        ));
                }
                dr.Close();
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<int> GetStatisticalForecastYears()
        {
            SqlConnection con = Connect("DBConnectionString");

            try
            {
                string str = $"select distinct yearP from tblForecastTransactions where yearP >= {Weeks.CurrentYear()} order by yearP";
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

        public static List<ProductFamilies> GetAllProductFamilies()
        {
            SqlConnection con = Connect();

            try
            {
                string str = $"select * from tblProductFamilies";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<ProductFamilies> result = new List<ProductFamilies>();
                while (dr.Read())
                {
                    result.Add(new ProductFamilies(
                        Convert.ToString(dr["productFamilyId"]),
                        Convert.ToString(dr["productFamilyName"])
                        ));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Transactions> GetTransactions(string from = null, string to = null, string year = null, string quarter = null,
            string customerNumber = null, string shipToName = null, string itemNumber = null, string productFamilyId = null, string market = null, string countryId = null)
        {
            List<string> whereList = new List<string>();
            if (!(from is null)) whereList.Add($"forecastTransactionCreationDate >= '{string.Join(" ", from.Split('T'))}'");
            if (!(to is null)) whereList.Add($"forecastTransactionCreationDate <= '{string.Join(" ", to.Split('T'))}'");
            if (!(year is null)) whereList.Add($"yearP = {year}");
            if (!(quarter is null)) whereList.Add($"quarterP = {quarter}");
            if (!(customerNumber is null)) whereList.Add($"trans.customerNumber = {customerNumber}");
            if (!(shipToName is null)) whereList.Add($"trans.shipToName = '{shipToName}'");
            if (!(itemNumber is null)) whereList.Add($"trans.itemNumber = '{itemNumber}'");
            if (!(productFamilyId is null)) whereList.Add($"families.productFamilyId = '{productFamilyId}'");
            if (!(market is null)) whereList.Add($"countries.countryMarket = '{market}'");
            if (!(countryId is null)) whereList.Add($"countries.countryId = '{countryId}'");
            string whereStr = $"{(whereList.Count == 0 ? "" : $" where {string.Join(" AND ", whereList)}")}";

            SqlConnection con = Connect();

            try
            {
                string str = $"select trans.*, items.itemDescription, families.productFamilyName, customers.customerName, people.personFullName," +
                    $" countries.countryName, countries.countryMarket" +
                    $" from tblForecastTransactions as trans" +
                    $" left join tblItems as items on trans.itemNumber = items.itemNumber" +
                    $" left join tblProductFamilies as families on items.productFamilyId = families.productFamilyId" +
                    $" left join tblCustomers as customers on trans.customerNumber = customers.customerNumber" +
                    $" left join tblPeople as people on customers.saleMenagerId = people.personId" +
                    $" left join tblShipTos as ST on(trans.customerNumber = ST.customerNumber and trans.shipToName = ST.shipToName)" +
                    $" left join tblCountries as countries on ST.countryId = countries.countryId{whereStr}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) { throw new Exception("NotFound"); }
                List<Transactions> result = new List<Transactions>();
                while (dr.Read())
                {
                    result.Add(new Transactions(
                        Convert.ToDateTime(dr["forecastTransactionCreationDate"]),
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["quarterP"]),
                        Convert.ToString(dr["forecastTransactionStatus"]),
                        Convert.ToDouble(dr["forecastTransactionQty"]),
                        Convert.ToString(dr["itemNumber"]),
                        Convert.ToString(dr["itemDescription"]),
                        Convert.ToString(dr["productFamilyName"]),
                        Convert.ToInt32(dr["customerNumber"]),
                        Convert.ToString(dr["customerName"]),
                        Convert.ToString(dr["shipToName"]),
                        Convert.ToBoolean(dr["forecastTransactionIsMelting"])? "Melting": "Classic",
                        Convert.ToString(dr["personFullName"]),
                        Convert.ToString(dr["countryName"]),
                        Convert.ToString(dr["countryMarket"]),
                        Convert.ToString(dr["forecastTransactionCreatedBy"]),
                        Convert.ToDateTime(dr["forecastTransactionLastUpdateDate"]),
                        Convert.ToString(dr["forecastTransactionLastUpdateBy"])
                        ));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Distribution GetDistribution()
        {
            SqlConnection con = Connect();

            try
            {
                string str = $"select * from tblFrProductFamilies left join tblProductFamilies on tblFrProductFamilies.frProductFamilyId = tblProductFamilies.productFamilyId";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Distribution result = new Distribution();
                while (dr.Read())
                {
                    if (Convert.ToString(dr["productFamilyName"]) == "Fructose S") result.S = Convert.ToDouble(dr["frProductFamilyDistribution"]);
                    if (Convert.ToString(dr["productFamilyName"]) == "Fructose N") result.N = Convert.ToDouble(dr["frProductFamilyDistribution"]);
                    if (Convert.ToString(dr["productFamilyName"]) == "Fructose D") result.D = Convert.ToDouble(dr["frProductFamilyDistribution"]);
                    if (Convert.ToString(dr["productFamilyName"]) == "Fructose E") result.E = Convert.ToDouble(dr["frProductFamilyDistribution"]);
                    if (Convert.ToString(dr["productFamilyName"]) == "Fructose OF") result.OF = Convert.ToDouble(dr["frProductFamilyDistribution"]);
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static bool PostInventoryParams(Dictionary<string, double> distD, List<Periods> periods)
        {
            SqlConnection con = Connect();

            try
            {
                string str = $"";
                foreach (string key in distD.Keys) str += $"update tblFrProductFamilies set frProductFamilyDistribution = {distD[key].ToString().Replace(",", ".")}" +
                        $" where frProductFamilyId = (select productFamilyId from tblProductFamilies where productFamilyName = 'Fructose {key}')\n";
                foreach (Periods p in periods) str += $"update tblPeriods set dailyProduction = {p.DailyProduction}, greensUsing = {p.GreensUsing}, factorS = {p.FactorS}" +
                        $", factorN = {p.FactorN}, factorD = {p.FactorD}, factorE = {p.FactorE}, factorOF = {p.FactorOF}, factorL = {p.FactorL}" +
                        $", storageCapacity = {p.StorageCapacity}, conteinersInventory = {p.ConteinersInventory} where yearP = {p.YearP} and quarterP = {p.QuarterP}\n";
                str = $"BEGIN TRANSACTION\n{str}IF @@ERROR = 0 COMMIT ELSE ROLLBACK";
                SqlCommand cmd = CreateCommand(str, con);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<int, Weeks> GetYearWeeks(int year)
        {
            SqlConnection con = Connect();

            try
            {
                string str = $"select * from tblWeeks where yearP = {year}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<int, Weeks> result = new Dictionary<int, Weeks>();
                while (dr.Read())
                {
                    Weeks tmp = new Weeks(Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["quarterP"]),
                        Convert.ToInt32(dr["weekNumber"]),
                        Convert.ToDouble(dr["actualProduction"]),
                        Convert.ToDouble(dr["actualGreens"]),
                        Convert.ToDouble(dr["actualSales"]),
                        Convert.ToDouble(dr["actualInventory"]),
                        Convert.ToDouble(dr["actualInventoryS"]),
                        Convert.ToDouble(dr["actualInventoryN"]),
                        Convert.ToDouble(dr["actualInventoryD"]),
                        Convert.ToDouble(dr["actualInventoryE"]),
                        Convert.ToDouble(dr["actualInventoryOF"]),
                        Convert.ToDouble(dr["actualInventoryExternal"]), true);
                    result[tmp.ThisAbsWeekNumber] = tmp;
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Weeks GetLastWeeks()
        {
            SqlConnection con = Connect();

            try
            {
                string str = $"select* from tblWeeks as t1 left join tblWeeks as t2 on(t1.yearP* 100 + t1.weekNumber<t2.yearP* 100 + t2.weekNumber) where t2.yearP is null";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows) throw new Exception("NotFound");
                dr.Read();
                return new Weeks(Convert.ToInt32(dr["yearP"]),
                    Convert.ToInt32(dr["quarterP"]),
                    Convert.ToInt32(dr["weekNumber"]),
                    Convert.ToDouble(dr["actualProduction"]),
                    Convert.ToDouble(dr["actualGreens"]),
                    Convert.ToDouble(dr["actualSales"]),
                    Convert.ToDouble(dr["actualInventory"]),
                    Convert.ToDouble(dr["actualInventoryS"]),
                    Convert.ToDouble(dr["actualInventoryN"]),
                    Convert.ToDouble(dr["actualInventoryD"]),
                    Convert.ToDouble(dr["actualInventoryE"]),
                    Convert.ToDouble(dr["actualInventoryOF"]),
                    Convert.ToDouble(dr["actualInventoryExternal"]), true);
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<int, Periods> GetPeriods(int lastWeekPeriod)
        {
            SqlConnection con = Connect("DBConnectionString");
            string whereStr = $" WHERE (yearP - {Periods.FirstYear}) * 4 + quarterP >= {lastWeekPeriod}";

            try
            {
                string str = $"SELECT * FROM tblPeriods{whereStr}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<int, Periods> result = new Dictionary<int, Periods>();
                while (dr.Read())
                {
                    Periods tmp = new Periods(
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
                        Convert.ToInt32(dr["conteinersInventory"]));
                    result[tmp.ThisPeriodNumber] = tmp;
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static Dictionary<int, ForecastSum> GetForecastSum(int yearFrom, int yearTo)
        {
            SqlConnection con = Connect("DBConnectionString");

            try
            {
                string str = $"WITH tbl AS(SELECT * FROM tblForecastTransactions WHERE forecastTransactionStatus = 'Completed' AND yearP >= {yearFrom} AND yearP <= {yearTo})\n" +
                    $"select sub.yearP, sub.quarterP, sum(sub.qty * cf.coefficientS) as S," +
                    $" sum(sub.qty * cf.coefficientN) as N," +
                    $" sum(sub.qty * cf.coefficientD) as D," +
                    $" sum(sub.qty * cf.coefficientE) as E," +
                    $" sum(sub.qty * cf.coefficientOF) as [OF]," +
                    $" sum(sub.qty * cf.coefficientL) as L" +
                    $" from(SELECT t1.yearP, t1.quarterP, tblProductFamilies.productFamilyId, tblProductFamilies.productFamilyName, sum(t1.forecastTransactionQty) as qty" +
                    $" FROM tbl t1 LEFT JOIN tbl t2" +
                    $" ON(t1.itemNumber = t2.itemNumber AND t1.yearP = t2.yearP AND t1.quarterP = t2.quarterP AND t1.customerNumber = t2.customerNumber" +
                    $" AND t1.shipToName = t2.shipToName AND t1.forecastTransactionCreationDate < t2.forecastTransactionCreationDate) LEFT JOIN tbl t3" +
                    $" ON(t1.itemNumber = t3.itemNumber AND t1.yearP = t3.yearP AND t1.quarterP = t3.quarterP AND t1.customerNumber = t3.customerNumber" +
                    $" AND t1.shipToName = t3.shipToName AND t1.forecastTransactionsId < t3.forecastTransactionsId)" +
                    $" LEFT JOIN tblItems ON t1.itemNumber = tblItems.itemNumber" +
                    $" LEFT JOIN tblProductFamilies ON tblItems.productFamilyId = tblProductFamilies.productFamilyId" +
                    $" WHERE t2.forecastTransactionCreationDate IS NULL AND t3.forecastTransactionsId IS NULL" +
                    $" GROUP BY tblProductFamilies.productFamilyId, tblProductFamilies.productFamilyName, t1.yearP, t1.quarterP) as sub" +
                    $" left join tblFrProductFamilies as cf on sub.productFamilyId = cf.frProductFamilyId" +
                    $" group by sub.yearP, sub.quarterP";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                Dictionary<int, ForecastSum> result = new Dictionary<int, ForecastSum>();
                while (dr.Read())
                {
                    ForecastSum tmp = new ForecastSum(
                        Convert.ToInt32(dr["yearP"]),
                        Convert.ToInt32(dr["quarterP"]),
                        Convert.ToDouble(dr["S"]),
                        Convert.ToDouble(dr["N"]),
                        Convert.ToDouble(dr["D"]),
                        Convert.ToDouble(dr["E"]),
                        Convert.ToDouble(dr["OF"]),
                        Convert.ToDouble(dr["L"]));
                    result[tmp.PeriodNumber] = tmp;
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }
    }
}