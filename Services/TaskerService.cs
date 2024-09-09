using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasker099.Model;
using Tasker099.Services;
using Microsoft.Maui.Storage;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Tasker099.Services
{
    public class TaskerService
    {
        List<Tasker> taskerList = new();

        public async Task<List<Tasker>> GetTaskers()
        {
            if (taskerList.Count > 0)
            {
                taskerList.Clear();
            }
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "v4_1.json");
            if (!File.Exists(filePath))
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("v4_1.json");
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);
            }
            

            var json = await File.ReadAllTextAsync(filePath);
            
            var allTaskers = JsonConvert.DeserializeObject<List<Tasker>>(json);
            
            taskerList = allTaskers?.Where(t => t.Name != "NULL").ToList() ?? [];
            return taskerList;
        }

        public async Task AddTasker(string name, string type, string? interval, string? intervalReset, string? date, string? resetDay)
        {
            var taskers = await GetTaskers();
            var tasker = new Tasker();
            //if name is unique
            if (tasker != null)
            {
                tasker.Name = name;
                tasker.Type = type;
                tasker.Interval = interval ?? "None";
                tasker.IntervalReset = intervalReset ?? "None";
                tasker.Date = date ?? "1990-10-10";
                tasker.ResetDay = resetDay ?? "None";
                tasker.Checked = false;
                tasker.CheckedTime = "None";
                tasker.DisplayText = "None";
                tasker.ImgSource = "chk_off_50.png";

                taskers.Add(tasker);
                var filePath = Path.Combine(FileSystem.AppDataDirectory, "v4_1.json");
                var json = JsonConvert.SerializeObject(taskers, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
            }

        }

        public static async Task UpdateTasker(string name, string? type, string? interval, string? intervalReset, string? date, string? resetDay, bool? checkedStatus, string? checkedTime)
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "v4_1.json");
            var json = await File.ReadAllTextAsync(filePath);
            var taskers = JsonConvert.DeserializeObject<List<Tasker>>(json);

            var tasker = taskers.FirstOrDefault(t => t.Name == name);
            if (tasker != null)
            {
                tasker.Type = type ?? tasker.Type;
                tasker.Interval = interval ?? tasker.Interval;
                tasker.IntervalReset = intervalReset ?? tasker.IntervalReset;
                tasker.Date = date ?? tasker.Date;
                tasker.ResetDay = resetDay ?? tasker.ResetDay;
                tasker.Checked = checkedStatus ?? tasker.Checked;
                
                tasker.CheckedTime = checkedTime ?? tasker.CheckedTime;

                json = JsonConvert.SerializeObject(taskers, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        public async Task DeleteTasker(string name)
        {
            if (name is null)
                return;

            var filePath = Path.Combine(FileSystem.AppDataDirectory, "v4_1.json");
            var json = await File.ReadAllTextAsync(filePath);
            var taskers = JsonConvert.DeserializeObject<List<Tasker>>(json);

            //search by tasker.Name and if found, delete that specific Tasker from taskers
            if (taskers != null)
            {
                var taskerToRemove = taskers.FirstOrDefault(t => t.Name == name);
                if (taskerToRemove != null)
                {
                    taskers.Remove(taskerToRemove);

                    json = JsonConvert.SerializeObject(taskers, Formatting.Indented);
                    await File.WriteAllTextAsync(filePath, json);
                }
            }

        }

        // get TaskerList and update DisplayText field based on Type being date or interval  using a nested if, but keep the nested if a placeholder, give me the outer template of it
        public async Task<List<Tasker>> UpdateDisplayText(List<Tasker> taskers = null)
        {
            taskers ??= await GetTaskers();
            foreach (var tasker in taskers)
            {
                if (tasker.Name == "NULL")
                    break;
                if (tasker.Type is null)
                    break;
                if (string.IsNullOrEmpty(tasker.Interval) && string.IsNullOrEmpty(tasker.IntervalReset) && string.IsNullOrEmpty(tasker.ResetDay) && string.IsNullOrEmpty(tasker.Date))
                    break;
                if (tasker.Type == "date")
                {
                    // Check time left to tasker.date from current day (not time, just the day)
                    // If time left is less than 1 day, display time left as 0D Left, if overdue, just say Outdated.
                    // tasker.date field is a yyyy-MM-dd but is stored as a String.
                    
                    DateTime taskDate = DateTime.Parse(tasker.Date);
                    DateTime now = DateTime.Now.Date;
                    if (taskDate < now)
                    {
                        tasker.DisplayText = "Outdated";
                        if (tasker.Checked == true && tasker.CheckedTime != "None")
                        {
                            tasker.Checked = false;
                            tasker.CheckedTime = "None";
                        }
                    }
                    else
                    {
                        TimeSpan remainingTime = taskDate - now;
                        if (remainingTime.TotalHours < 24)
                            tasker.DisplayText = "Today";
                        else
                        {
                            int monthsLeft = (taskDate.Year - now.Year) * 12 + taskDate.Month - now.Month;
                            int daysLeft = taskDate.Day - now.Day;
                            if (daysLeft < 0)
                            {
                                monthsLeft--;
                                daysLeft += DateTime.DaysInMonth(now.Year, now.Month);
                            }
                            tasker.DisplayText = monthsLeft > 0 ? $"{monthsLeft}M {daysLeft}D Left" : $"{daysLeft}D Left";
                        }
                    }
                }
                else if (tasker.Type == "interval") 
                { 
                    tasker.Date = "1990-10-10";
                    if (tasker.Interval == "SpecificDay")
                    {
                        //ResetDay writes a specific day, 1st, 2nd, 30th, 16th, etc, this means an interval to that date every month.
                        //Calculate days left till that specific day
                        //If days left is less than 1 day, display 0D Left, CANT be outdated, because there won't be a year or month specified.
                        int dayOfMonth = int.Parse(tasker.ResetDay.TrimEnd('s', 't', 'n', 'd', 'r', 'h'));
                        if (dayOfMonth > 0 && dayOfMonth <= 31)
                        {

                            DateTime today = DateTime.Now.Date;
                            DateTime nextSpecificDay = new DateTime(today.Year, today.Month, dayOfMonth);

                            if (nextSpecificDay < today)
                            {
                                nextSpecificDay = nextSpecificDay.AddMonths(1);
                                if (tasker.Checked == true && tasker.CheckedTime != "None")
                                {
                                    tasker.Checked = false;
                                    tasker.CheckedTime = "None";
                                }
                            }

                            TimeSpan remainingTime = nextSpecificDay - today;
                            if (remainingTime.TotalHours < 24)
                                tasker.DisplayText = "Today";
                            else
                            {
                                int monthsLeft = (nextSpecificDay.Year - today.Year) * 12 + nextSpecificDay.Month - today.Month;
                                int daysLeft = nextSpecificDay.Day - today.Day;
                                if (daysLeft < 0)
                                {
                                    monthsLeft--;
                                    daysLeft += DateTime.DaysInMonth(today.Year, today.Month);
                                }
                                tasker.DisplayText = monthsLeft > 0 ? $"{monthsLeft}M {daysLeft}D Left" : $"{daysLeft}D Left";
                            }
                        }
                        else { }
                    }
                    else
                    {
                        if (tasker.Interval == "Daily")
                        {
                            tasker.DisplayText = "Daily";
                            if (tasker.Checked == true && tasker.CheckedTime != "None")
                            {
                                if (DateTime.Parse(tasker.CheckedTime) < DateTime.Now.Date) { 
                                    tasker.Checked = false;
                                    tasker.CheckedTime = "None";
                                }
                            }
                        }
                        else
                        {
                            /* Daily is taken care of already, values can only be Weekly or Monthly. Means every week every month 1st.
                             * Weekly calculates to next Monday, Monthly calculates to next 1st of the month.
                             * if the day is today, display 0D Left, can't be lower than 0D because its an interval.
                             */

                            if (tasker.Interval == "Weekly")
                            {
                                DateTime today = DateTime.Now.Date;
                                DateTime nextDay;
                                if (tasker.IntervalReset == "None")
                                    nextDay = today.AddDays((int)DayOfWeek.Monday - (int)today.DayOfWeek);
                                else
                                {
                                    DayOfWeek resetDayOfWeek = Enum.Parse<DayOfWeek>(tasker.IntervalReset);
                                    nextDay = today.AddDays((int)resetDayOfWeek - (int)today.DayOfWeek);
                                }

                                if (nextDay < today)
                                    nextDay = nextDay.AddDays(7);

                                TimeSpan remainingTime = nextDay - today;
                                if (remainingTime.TotalHours < 24)
                                    tasker.DisplayText = "Today";
                                else
                                {
                                    int monthsLeft = (nextDay.Year - today.Year) * 12 + nextDay.Month - today.Month;
                                    int daysLeft = nextDay.Day - today.Day;
                                    if (daysLeft < 0)
                                    {
                                        monthsLeft--;
                                        daysLeft += DateTime.DaysInMonth(today.Year, today.Month);
                                    }
                                    tasker.DisplayText = monthsLeft > 0 ? $"{monthsLeft}M {daysLeft}D Left" : $"{daysLeft}D Left";
                                }
                            }
                            else if (tasker.Interval == "Monthly")
                            {
                                DateTime today = DateTime.Now.Date;
                                DateTime nextFirst = new DateTime(today.Year, today.Month, 1);
                                if (nextFirst < today)
                                {
                                    nextFirst = nextFirst.AddMonths(1);
                                    if (tasker.Checked == true && tasker.CheckedTime != "None")
                                    {
                                        tasker.Checked = false;
                                        tasker.CheckedTime = "None";
                                    }
                                }
                                TimeSpan remainingTime = nextFirst - today;
                                if (remainingTime.TotalHours < 24)
                                    tasker.DisplayText = "0D Left";
                                else
                                {
                                    int monthsLeft = (nextFirst.Year - today.Year) * 12 + nextFirst.Month - today.Month;
                                    int daysLeft = nextFirst.Day - today.Day;
                                    if (daysLeft < 0)
                                    {
                                        monthsLeft--;
                                        daysLeft += DateTime.DaysInMonth(today.Year, today.Month);
                                    }
                                    tasker.DisplayText = monthsLeft > 0 ? $"{monthsLeft}M {daysLeft}D Left" : $"{daysLeft}D Left";
                                }
                            }
                        }
                        }
                    }
                }
            return taskers;
        }

        // filter out TaskerList to only display Interval or Date type taskers, and return the filtered taskerList.
        public async Task<List<Tasker>> FilterTaskers(string type, List<Tasker>? taskers = null)
        {
            taskers ??= await GetTaskers();
            
            if (type != "date" && type != "interval")
                return taskers;

            return taskers.Where(t => t.Type == type).ToList();
        }

        // get TaskerList and sort the taskers in it, according to given 'Field name' like Interval, Date, Name. Etc. And return sorted taskerList.
        public async Task<List<Tasker>> SortTaskers(List<Tasker> taskers = null, string fieldName = "DisplayText")
        {
            taskers ??= await GetTaskers();
            //make sure fieldname is 1 of the 3, Interval, Date, Name
            if (fieldName != "Interval" && fieldName != "DisplayText" && fieldName != "Name")
                return taskers;

            switch (fieldName)
            {
                case "DisplayText":
                    /* DisplayText is a complex field, it can contain "Today", "Daily", "1D Left", "1M Left"..
                    * So the idea is this format
                    * Daily/Today
                    * 1D Left and higher
                    */
                    return taskers.OrderBy(t => GetDisplayTextSortValue(t.DisplayText)).ToList();

                default:
                    return taskers;
            }
        }
        private int GetDisplayTextSortValue(string? displayText)
        {
            if (displayText == "Daily")
                return 0;
            if (displayText == "Today")
                return 1;

            // Extract the number of days or months left
            var match = Regex.Match(displayText, @"(\d+)([DM]) Left");
            if (match.Success)
            {
                int value = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;

                // Days are more urgent than months
                if (unit == "D")
                    return 2 + value; // Offset by 2 to place after "Daily" and "Today"
                if (unit == "M")
                    return 1000 + value; // Offset by 1000 to place after days
            }

            // If the format is unrecognized, place it at the end
            return int.MaxValue;
        }

        //A method to save the AppDataDirectory json back to the original json in AppPackage
    }
}
