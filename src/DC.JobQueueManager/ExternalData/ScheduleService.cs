using System;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces.ExternalData;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.JobQueueManager.ExternalData
{
    public sealed class ScheduleService : IScheduleService
    {
        private readonly ILogger _logger;

        public ScheduleService(ILogger logger)
        {
            _logger = logger;
        }

        public bool CanExecuteSchedule(Schedule schedule, DateTime nowUtc, bool removeOldDates)
        {
            DateTime? nextRun = GetNextRun(nowUtc, schedule, removeOldDates);
            if (removeOldDates && nextRun == null)
            {
                // Optimisation, schedule cannot run today so skip. Don't do this if we want to run old dates that may be far behind us.
                return false;
            }

            if (nextRun > nowUtc)
            {
                // We can't run future.
                return false;
            }

            if (schedule.LastExecuteDateTime.HasValue)
            {
                DateTime lastExecutionDateTime = schedule.LastExecuteDateTime.Value.TrimSeconds();
                if (nextRun <= lastExecutionDateTime)
                {
                    // Don't run because the calculated next run is the same as the one we did run.
                    return false;
                }
            }

            return true;
        }

        public DateTime? GetNextRun(DateTime nowUtc, Schedule schedule, bool removeOldDates)
        {
            int minute = nowUtc.Minute;
            int hour = nowUtc.Hour;
            int day = nowUtc.Day;
            int month = nowUtc.Month;

            if (schedule.Month.HasValue)
            {
                month = schedule.Month.Value;
                if (removeOldDates && month != nowUtc.Month)
                {
                    return null;
                }
            }

            if (schedule.DayOfTheMonth.HasValue)
            {
                day = schedule.DayOfTheMonth.Value;
                if (removeOldDates && !schedule.DayOfTheWeek.HasValue && day < nowUtc.Day)
                {
                    return null;
                }
            }

            if (schedule.Hour.HasValue)
            {
                hour = schedule.Hour.Value;
                if (removeOldDates && hour < nowUtc.Hour)
                {
                    return null;
                }
            }

            if (schedule.Minute.HasValue)
            {
                minute = schedule.Minute.Value;
                if (schedule.MinuteIsCadence ?? false)
                {
                    minute = GetNextMinuteBoundary(nowUtc.Minute, minute);
                }

                if (removeOldDates && minute < nowUtc.Minute)
                {
                    return null;
                }
            }

            // Do this last
            if (schedule.DayOfTheWeek.HasValue)
            {
                if (!Enum.IsDefined(typeof(DayOfWeek), (int)schedule.DayOfTheWeek))
                {
                    _logger.LogWarning($"{nameof(schedule.DayOfTheWeek)} is not a valid {nameof(DayOfWeek)} for schedule {schedule.ID}, skipping this part of the rule");
                }
                else
                {
                    DateTime nextPossibleDate = GetNextWeekday(nowUtc, (DayOfWeek)schedule.DayOfTheWeek.Value);
                    if (schedule.Month.HasValue)
                    {
                        if (nextPossibleDate.Month == month)
                        {
                            if (schedule.DayOfTheMonth.HasValue)
                            {
                                // Both days are valid, so we choose the minimum.
                                day = Math.Min(day, nextPossibleDate.Day);
                            }
                            else
                            {
                                // The month was specified, and the next specified day is in the month specified so use it.
                                day = nextPossibleDate.Day;
                            }
                        }
                        else
                        {
                            if (!schedule.DayOfTheMonth.HasValue)
                            {
                                // A month was specified, but we can't find a day in the specified month, so reset the day so we don't try and run.
                                if (removeOldDates)
                                {
                                    return null;
                                }

                                day = 1;
                            }
                        }
                    }
                    else
                    {
                        if (schedule.DayOfTheMonth.HasValue)
                        {
                            DateTime dayOfMonth = new DateTime(
                                nowUtc.Year,
                                nowUtc.Month,
                                schedule.DayOfTheMonth.Value,
                                nowUtc.Hour,
                                nowUtc.Minute,
                                0);
                            if (dayOfMonth < nowUtc)
                            {
                                // The specified day is too early in the month (we have past it) so we take this one.
                                day = nextPossibleDate.Day;
                            }
                            else
                            {
                                // Both days are valid, so we choose the minimum.
                                day = Math.Min(day, nextPossibleDate.Day);
                            }
                        }
                        else
                        {
                            // No day was specified, so we take the next specified day.
                            day = nextPossibleDate.Day;
                        }

                        // No month was specified, so we take the calculated month.
                        month = nextPossibleDate.Month;

                        if (removeOldDates && month != nowUtc.Month)
                        {
                            // If the month is not this month, then ignore.
                            return null;
                        }
                    }
                }
            }

            DateTime nextRun = new DateTime(DateTime.UtcNow.Year, month, day, hour, minute, 0, DateTimeKind.Utc);

            return nextRun;
        }

        private DateTime GetNextWeekday(DateTime nowUtc, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)nowUtc.DayOfWeek + 7) % 7;
            return nowUtc.AddDays(daysToAdd);
        }

        private int GetNextMinuteBoundary(int currentMinute, int boundary)
        {
            int match = 0; // > 55 should wrap around to 0
            for (int i = 0; i < 60; i += boundary)
            {
                if (i < currentMinute)
                {
                    continue;
                }

                match = i;
                break;
            }

            return match;
        }
    }
}
