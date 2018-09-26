using System;
using ESFA.DC.JobQueueManager.Data.Entities;

namespace ESFA.DC.JobQueueManager.Interfaces.ExternalData
{
    public interface IScheduleService
    {
        bool CanExecuteSchedule(Schedule schedule, DateTime nowUtc, bool removeOldDates);
    }
}
