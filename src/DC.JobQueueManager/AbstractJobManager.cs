using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public abstract class AbstractJobManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IReturnCalendarService _returnCalendarService;
        private readonly IDateTimeProvider _dateTimeProvider;

        protected AbstractJobManager(
            DbContextOptions contextOptions,
            IReturnCalendarService returnCalendarService,
            IDateTimeProvider dateTimeProvider)
        {
            _contextOptions = contextOptions;
            _returnCalendarService = returnCalendarService;
            _dateTimeProvider = dateTimeProvider;
        }

        public bool IsCrossLoadingEnabled(JobType jobType)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.JobTypes.SingleOrDefault(x => x.JobTypeId == (short)jobType);
                return entity != null && entity.IsCrossLoadingEnabled;
            }
        }

        public ReturnPeriod GetReturnPeriod(string collectionName, DateTime dateTimeUtc)
        {
            return _returnCalendarService.GetPeriodAsync(collectionName, _dateTimeProvider.ConvertUtcToUk(dateTimeUtc)).Result;
        }

        public ReturnPeriod GetNextReturnPeriod(string collectionName)
        {
            return _returnCalendarService.GetNextPeriodAsync(collectionName).Result;
        }
    }
}
