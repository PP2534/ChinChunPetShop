using ChinChunPetShop.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChinChunPetShop.Models.Configs
{
    public class DBConfig
    {
        protected ChinChunPetShopContext db;
        public string message = "";

        public DBConfig(ChinChunPetShopContext context)
        {
            db = context;
        }

        public ChinChunPetShopContext GetDB()
        {
            return db;
        }

        public string GetID(string key, int id, int len = 8)
        {
            int zerosNeeded = len - key.Length - id.ToString().Length;
            if (zerosNeeded < 0)
                throw new ArgumentException("The total length (len) is too small for the given key and id.");

            return key + new string('0', zerosNeeded) + id.ToString();
        }

        public List<string> GetDSID(string key, int start, int end, int len = 8)
        {
            List<string> list = new List<string>();
            for (int i = start; i <= end; i++)
            {
                list.Add(GetID(key, i));
            }
            return list;
        }

        public async Task<List<string>> GetVTS()
        {
            return await db.VaiTros.Select(m => m.MaVT).ToListAsync();
        }

        public List<DateTime> getStartEndTime(string field = "cd", string key = "week")
        {
            DateTime today = DateTime.Now.Date;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            int quarterIndex = (today.Month - 1) / 3;
            int firstMonthOfQuarter = quarterIndex * 3 + 1;
            DateTime startCurrentTime = today.AddDays(-diff);
            DateTime endCurrentTime = DateTime.Now;
            DateTime startPreviousTime = startCurrentTime.AddDays(-7);
            DateTime endPreviousTime = startCurrentTime.AddDays(-1);
            if (field == "xtd")
            {
                startCurrentTime = today.AddDays(-diff);
                startPreviousTime = today.AddDays(-diff-7);
                endPreviousTime = today.AddDays(-7);
                if (key == "month")
                {
                    startCurrentTime = new DateTime(today.Year, today.Month, 1);
                    startPreviousTime = startCurrentTime.AddMonths(-1);
                    endPreviousTime = today.AddMonths(-1);
                }
                else if (key == "quarter")
                {
                    startCurrentTime = new DateTime(today.Year, firstMonthOfQuarter, 1);
                    double days = (endCurrentTime - startCurrentTime).TotalDays;
                    startPreviousTime = startCurrentTime.AddMonths(-3);
                    endPreviousTime = startPreviousTime.AddDays(days);
                }
                else if (key == "year")
                {
                    startCurrentTime = new DateTime(today.Year, 1, 1);
                    startPreviousTime = startCurrentTime.AddYears(-1);
                    endPreviousTime = new DateTime(startPreviousTime.Year, today.Month, today.Day);
                }
            }
            else if(field == "rolling")
            {
                startCurrentTime = today.AddDays(-6);
                startPreviousTime = today.AddDays(-7);
                endPreviousTime = startCurrentTime.AddDays(-1);
                if (key == "month")
                {
                    startCurrentTime = today.AddDays(-29);
                    startPreviousTime = today.AddDays(-30);
                    endPreviousTime = startCurrentTime.AddDays(-1);
                }
                else if (key == "quarter")
                {
                    startCurrentTime = today.AddDays(-89);
                    startPreviousTime = today.AddDays(-90);
                    endPreviousTime = startCurrentTime.AddDays(-1);
                }
                else if (key == "year")
                {
                    startCurrentTime = today.AddDays(-364);
                    startPreviousTime = today.AddDays(-365);
                    endPreviousTime = startCurrentTime.AddDays(-1);
                }
            }
            else
            {
                //    startCurrentTime = today.AddDays(-diff);
                //    startPreviousTime = startCurrentTime.AddDays(-7);
                if (key == "month")
                {
                    startCurrentTime = new DateTime(today.Year, today.Month, 1);
                    startPreviousTime = startCurrentTime.AddMonths(-1);
                }
                else if (key == "quarter")
                {
                    startCurrentTime = new DateTime(today.Year, firstMonthOfQuarter, 1);
                    startPreviousTime = startCurrentTime.AddMonths(-3);
                }
                else if (key == "year")
                {
                    startCurrentTime = new DateTime(today.Year, 1, 1);
                    startPreviousTime = startCurrentTime.AddYears(-1);
                }

            }
           
            return [startCurrentTime,endCurrentTime,startPreviousTime, endPreviousTime];
        }

        public List<KeyValuePair<string, DateTime>> getKeyTime(string key = "week", int lengh = 7)
        {
            DateTime today = DateTime.Now.Date;
            List<string> weeks = ["T2", "T3", "T4", "T5", "T6", "T7", "CN"];
            List<string> months = ["T1", "T2","T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12"];
            List<string> quarters = ["I", "II","III", "IV"];
            List<KeyValuePair<string, DateTime>> ans = new List<KeyValuePair<string, DateTime>>();
            if (key == "week")
            {
                int dayOfWeek = (int)today.DayOfWeek;
                string weekday = weeks[dayOfWeek == 0 ? 6 : dayOfWeek - 1];
                ans.Add(new KeyValuePair<string, DateTime>(weekday,today));
                lengh--;
                while (lengh > 0) {
                    today = today.AddDays(-1);
                    dayOfWeek = (int)today.DayOfWeek;
                    weekday = weeks[dayOfWeek == 0 ? 6 : dayOfWeek - 1];
                    ans.Insert(0,new KeyValuePair<string, DateTime>(weekday, today));
                    lengh--;
                }
                return ans;
            }
            if(key == "month")
            {
                today = new DateTime(today.Year, today.Month, 1);
                ans.Add(new KeyValuePair<string, DateTime>(months[today.Month-1] + "/" + today.Year, today));
                lengh--;
                while (lengh > 0)
                {
                    today = today.AddMonths(-1);
                    ans.Insert(0,new KeyValuePair<string, DateTime>(months[today.Month - 1]+"/"+today.Year,today));
                    lengh--;
                }
                return ans;
            }
            if (key == "quarter")
            {
                int quarterIndex = (today.Month - 1) / 3;
                int firstMonthOfQuarter = quarterIndex * 3 + 1;
                today = new DateTime(today.Year, firstMonthOfQuarter, 1);
                ans.Add(new KeyValuePair<string, DateTime>(quarters[(today.Month - 1)/3] + "/" + today.Year, today));
                lengh--;
                while (lengh > 0)
                {
                    today = today.AddMonths(-3);
                    quarterIndex = (today.Month - 1) / 3;
                    firstMonthOfQuarter = quarterIndex * 3 + 1;
                    ans.Insert(0,new KeyValuePair<string, DateTime>(quarters[(today.Month - 1) / 3] + "/" + today.Year, new DateTime(today.Year, firstMonthOfQuarter, 1)));
                    lengh--;
                }
                return ans;
            }
            if(key == "year")
            {
                today = new DateTime(today.Year, 1, 1);
                ans.Add(new KeyValuePair<string, DateTime>($"{today.Year}", today));
                lengh--;
                while (lengh > 0)
                {
                    today = today.AddYears(-1);
                    ans.Insert(0,new KeyValuePair<string, DateTime>($"{today.Year}", today));
                    lengh--;
                }
                return ans;
            }
            return new List<KeyValuePair<string, DateTime>>();
        } 


        public KeyValuePair<string, int> getTG(List<int> lt)
        {
            var avg = (int) lt.Average();
            if (avg >= 1000000)
            {
                return new KeyValuePair<string, int>("M", 1000000);
            }
            else if (avg >= 1000)
            {
                return new KeyValuePair<string, int>("K", 1000);
            }
            else
            {
                return new KeyValuePair<string, int>("", 1);
            }
        }
    }
}
