using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Shifts;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public static class TasksTimeStampHelper
    {
        public static LocalDateTime CorrectTasksTimeStamp(LocalDateTime tasksTimestamp, ShiftModel currentShift)
        {
            if (currentShift.IsOvernight)
            {
                // If we're after midnight
                if (tasksTimestamp.ToDateTimeUnspecified().TimeOfDay <= currentShift.EndTime)
                {
                    // Determine new timestamp
                    LocalDateTime newTasksTimestamp;

                    // If the settings timestamp is from the previous day we can use it
                    if (Settings.TasksTimestamp.ToDateTimeUnspecified().TimeOfDay >= currentShift.StartTime)
                    {
                        newTasksTimestamp = Settings.TasksTimestamp;
                    }
                    // Settings timestamp is wrong
                    else
                    {
                        // Go back one minute to get the prvious day
                        newTasksTimestamp = tasksTimestamp.PlusMinutes(-1);

                        // Save the new timestamp
                        Settings.TasksTimestamp = newTasksTimestamp;
                    }

                    // Save the new timestamp;
                    tasksTimestamp = newTasksTimestamp;
                }
            }
            // Else the time stamp is good

            return tasksTimestamp;
        }
    }
}
