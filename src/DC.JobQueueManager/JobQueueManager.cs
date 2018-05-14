using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Models;
using ESFA.DC.JobQueueManager.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ESFA.DC.JobQueueManager
{
    public sealed class JobQueueManager : IJobQueueManager
    {
        private readonly DbContextOptions _contextOptions;

        public JobQueueManager(DbContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public long AddJob(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = new JobEntity
                {
                    DateTimeSubmittedUtc = DateTime.UtcNow,
                    FileName = job.FileName,
                    JobType = (short)job.JobType,
                    Priority = job.Priority,
                    Status = (short)job.Status,
                    StorageReference = job.StorageReference,
                    Ukprn = job.Ukprn
                };
                context.Jobs.Add(entity);
                context.SaveChanges();
                return entity.JobId;
            }
        }

       public IEnumerable<Job> GetAllJobs()
        {
            var jobs = new List<Job>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntities = context.Jobs.ToList();
                jobEntities.ForEach(x =>
                    jobs.Add(JobConverter.Convert(x)));
            }

            return jobs;
        }

        public Job GetJobById(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var job = new Job();
                JobConverter.Convert(entity, job);
                return job;
            }
        }

        public Job GetJobByPriority()
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var job = context.Jobs.FromSql("sp_GetJobByPriority").FirstOrDefault();
                return JobConverter.Convert(job);
            }
        }

        public void RemoveJobFromQueue(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                if (entity.Status != 1) // if already moved, then dont delete
                {
                    throw new ArgumentOutOfRangeException($"Job is already moved from ready status, unable to delete");
                }

                context.Jobs.Remove(entity);
                context.SaveChanges();
            }
        }

        public bool UpdateJob(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == job.JobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {job.JobId} does not exist");
                }

                JobConverter.Convert(job, entity);
                entity.DateTimeUpdatedUtc = DateTime.UtcNow;
                context.Entry(entity).Property("RowVersion").OriginalValue = Convert.FromBase64String(job.RowVersion);
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    throw new Exception("Save failed. Job details have been changed. Reload the job object and try save again");
                }
            }
        }

        public bool UpdateJobStatus(long jobId, JobStatus status)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                entity.Status = (short)status;

                entity.DateTimeUpdatedUtc = DateTime.UtcNow;
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();
                return true;
            }
        }
    }
}