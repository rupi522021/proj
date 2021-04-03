using GalamForecast.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GalamForecast.Models
{
    public class Person
    {
        public Person(int id, string fullName, string email)
        {
            Id = id;
            FullName = fullName;
            Email = email;
        }

        public Person() { }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public static DbUpdates UpdatePeople()
        {
            try
            {
                List<Person> ErpData = ErpDBServices.GetErpPeople();
                Dictionary<int, Person> DbData = DBServices.GetPeople();
                List<Person> ToAdd = new List<Person>();
                List<Person> ToUpdate = new List<Person>();
                for (int i = 0; i < ErpData.Count; i++)
                {
                    if (!DbData.ContainsKey(ErpData[i].Id)) ToAdd.Add(ErpData[i]);
                    else
                    {
                        Person tmp = DbData[ErpData[i].Id];
                        if (tmp.FullName != ErpData[i].FullName || tmp.Email != ErpData[i].Email) ToUpdate.Add(ErpData[i]);
                    }
                }
                DbUpdates result = DBServices.AddUpdatePeople(ToAdd, ToUpdate);
                result.FunctionName = "UpdatePeople";
                result.Report = result.NumOfErrors > 0 ? "Error" : "Done";
                return result;
            }
            catch (Exception) { return new DbUpdates("UpdatePeople", "Error"); }
        }
    }
}