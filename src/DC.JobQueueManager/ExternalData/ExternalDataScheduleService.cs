using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces.ExternalData;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager.ExternalData
{
    public sealed class ExternalDataScheduleService : IExternalDataScheduleService
    {
        private readonly DbContextOptions<JobQueueDataContext> _contextOptions;

        private readonly IScheduleService _scheduleService;

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly ILogger _logger;

        public ExternalDataScheduleService(DbContextOptions<JobQueueDataContext> contextOptions, IScheduleService scheduleService, IDateTimeProvider dateTimeProvider, ILogger logger)
        {
            _contextOptions = contextOptions;
            _scheduleService = scheduleService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<IEnumerable<Jobs.Model.Enums.JobType>> GetJobs(bool removeOldDates, CancellationToken cancellationToken)
        {
            DateTime nowUtc = _dateTimeProvider.GetNowUtc().TrimSeconds();

            HashSet<int> jobs = new HashSet<int>();

            try
            {
                using (JobQueueDataContext db = new JobQueueDataContext(_contextOptions))
                {
                    IQueryable<Schedule> nonRun = db.Schedule.Where(x => x.Enabled);
                    foreach (Schedule schedule in nonRun)
                    {
                        if (!_scheduleService.CanExecuteSchedule(schedule, nowUtc, removeOldDates))
                        {
                            continue;
                        }

                        jobs.Add(schedule.JobTypeId);
                        if (schedule.ExecuteOnceOnly)
                        {
                            db.Schedule.Remove(schedule);
                        }
                        else
                        {
                            schedule.LastExecuteDateTime = nowUtc;
                        }
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve and process external data schedules", ex);
            }

            return jobs.Select(x => (Jobs.Model.Enums.JobType)x);
        }
    }
}
