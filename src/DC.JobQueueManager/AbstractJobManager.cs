using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected AbstractJobManager(DbContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public bool IsCrossLoadingEnabled(JobType jobType)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.JobTypes.SingleOrDefault(x => x.JobTypeId == (short)jobType);
                return entity != null && entity.IsCrossLoadingEnabled;
            }
        }
    }
}
