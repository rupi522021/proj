using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace GalamForecast.Models.DAL
{
    public class ErpDBServices
    {
        private static SqlConnection connect(string conString = "ErpDBConnectionString")
        {
            try
            {
                string cStr = WebConfigurationManager.ConnectionStrings[conString].ConnectionString;
                SqlConnection con = new SqlConnection(cStr);
                con.Open();
                return con;
            }
            catch { throw new Exception("con"); }
        }

        private static SqlCommand CreateCommand(string CommandSTR, SqlConnection con, int timeout = 30)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = CommandSTR;
            cmd.CommandTimeout = timeout;
            cmd.CommandType = System.Data.CommandType.Text;
            return cmd;
        }

        public static List<Countries> GetErpCountries()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM erpCountries";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<Countries> result = new List<Countries>();
                while (dr.Read()) { result.Add(new Countries(Convert.ToString(dr["countryId"]), Convert.ToString(dr["countryName"]), Convert.ToString(dr["countryMarket"]))); }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Person> GetErpPeople()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM erpPeople";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<Person> result = new List<Person>();
                while (dr.Read()) { result.Add(new Person(Convert.ToInt32(dr["personId"]), Convert.ToString(dr["personFullName"]), Convert.ToString(dr["personEmail"]))); }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<int> GetErpSaleMenagers()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM erpSaleMenager";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<int> result = new List<int>();
                while (dr.Read()) { result.Add(Convert.ToInt32(dr["personId"])); }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Customers> GetErpCustomers()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM erpCustomers";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<Customers> result = new List<Customers>();
                while (dr.Read())
                {
                    result.Add(new Customers(Convert.ToInt32(dr["customerNumber"]), Convert.ToString(dr["customerName"]), Convert.ToBoolean(dr["isLocal"]), Convert.ToInt32(dr["saleMenagerId"]), Convert.ToString(dr["countryId"])));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Items> GetErpItems()
        {
            SqlConnection con = connect();

            try
            {
                string str = $"SELECT * FROM erpItems";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<Items> result = new List<Items>();
                while (dr.Read())
                {
                    result.Add(new Items(Convert.ToString(dr["itemNumber"]), Convert.ToString(dr["itemDescription"]), Convert.ToString(dr["productFamilyId"])));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        //public static List<SaleRows> GetErpSaleRows()
        //{
        //    SqlConnection con = connect();

        //    try
        //    {
        //        string str = $"select year(saleDate) as year, DATEPART(QUARTER, saleDate) as quarter, customerNumber, itemNumber, sum(qty) as qty" +
        //            $" from erpSales" +
        //            $" where (year(saleDate) * 4 + DATEPART(QUARTER, saleDate)) < (year(GETDATE()) * 4 + DATEPART(QUARTER, GETDATE()))" +
        //            $" group by year(saleDate), DATEPART(QUARTER, saleDate), customerNumber, itemNumber";
        //        SqlCommand cmd = CreateCommand(str, con);
        //        SqlDataReader dr = cmd.ExecuteReader();
        //        List<SaleRows> result = new List<SaleRows>();
        //        while (dr.Read())
        //        {
        //            result.Add(new SaleRows(Convert.ToInt32(dr["year"]), Convert.ToInt32(dr["quarter"]), Convert.ToString(dr["itemNumber"]), Convert.ToInt32(dr["customerNumber"]), Convert.ToDouble(dr["qty"])));
        //        }
        //        return result;
        //    }
        //    catch (Exception ex) { throw ex; }
        //    finally { if (con != null) con.Close(); }
        //}

        public static List<SaleRows> GetErpSaleRows(int minPeriod, int maxPeriod)
        {
            SqlConnection con = connect();

            try
            {
                string str = $"select year(saleDate) as year, DATEPART(QUARTER, saleDate) as quarter, customerNumber, itemNumber, sum(qty) as qty" +
                    $" from erpSales" +
                    $" where ((year(saleDate) - 2013) * 4 + DATEPART(QUARTER, saleDate)) <= {maxPeriod} AND ((year(saleDate) - 2013) * 4 + DATEPART(QUARTER, saleDate)) >= {minPeriod}" +
                    $" group by year(saleDate), DATEPART(QUARTER, saleDate), customerNumber, itemNumber";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<SaleRows> result = new List<SaleRows>();
                while (dr.Read())
                {
                    result.Add(new SaleRows(Convert.ToInt32(dr["year"]), Convert.ToInt32(dr["quarter"]), Convert.ToString(dr["itemNumber"]), Convert.ToInt32(dr["customerNumber"]), Convert.ToDouble(dr["qty"])));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }

        public static List<Weeks> GetErpWeeks(int minAbsWeekNumber, int maxAbsWeekNumber)
        {
            SqlConnection con = connect();

            try
            {
                string str = $"select * from erpWeeks where yearP * 100 + weekNumber > {minAbsWeekNumber} and yearP * 100 + weekNumber < {maxAbsWeekNumber}";
                SqlCommand cmd = CreateCommand(str, con);
                SqlDataReader dr = cmd.ExecuteReader();
                List<Weeks> result = new List<Weeks>();
                while (dr.Read())
                {
                    result.Add(new Weeks(Convert.ToInt32(dr["yearP"]),
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
                        Convert.ToDouble(dr["actualInventoryExternal"])));
                }
                return result;
            }
            catch (Exception ex) { throw ex; }
            finally { if (con != null) con.Close(); }
        }
    }
}