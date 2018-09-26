using System;

namespace ESFA.DC.JobQueueManager
{
    public static class Extensions
    {
        public static DateTime TrimSeconds(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, DateTimeKind.Utc);
        }
    }
}
