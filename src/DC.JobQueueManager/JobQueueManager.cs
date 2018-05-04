using System;
using System.Collections.Generic;
using System.Linq;
using DC.JobQueueManager.Data;
using DC.JobQueueManager.Data.Entities;
using DC.JobQueueManager.Interfaces;
using DC.JobQueueManager.Models;
using DC.JobQueueManager.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DC.JobQueueManager
{
    public sealed class JobQueueManager : IJobQueueManager
    {
        private readonly IJobQueueManagerSettings _jobQueueManagerSettings;

        public JobQueueManager(IJobQueueManagerSettings jobQueueManagerSettings)
        {
            _jobQueueManagerSettings = jobQueueManagerSettings;
        }

        public long AddJob(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_jobQueueManagerSettings.ConnectionString))
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
                return job.JobId;
            }
        }

        public bool AnyInProgressReferenceJob()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Job> GetAllJobs()
        {
            var jobs = new List<Job>();
            using (var context = new JobQueueDataContext(_jobQueueManagerSettings.ConnectionString))
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

            using (var context = new JobQueueDataContext(_jobQueueManagerSettings.ConnectionString))
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
            throw new System.NotImplementedException();
        }

        public void RemoveJobFromQueue(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_jobQueueManagerSettings.ConnectionString))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                if (entity.Status != 1) // if already moved, then dont delete
                {
                    throw new Exception($"Job is already moved from ready status, unable to delete");
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

            var saved = false;

            using (var context = new JobQueueDataContext(_jobQueueManagerSettings.ConnectionString))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == job.JobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {job.JobId} does not exist");
                }

                JobConverter.Convert(job, entity);
                saved = SaveChanges(entity, context);
            }

            return saved;
        }

        public bool UpdateJobStatus(long jobId, JobStatus status)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            var saved = false;
            using (var context = new JobQueueDataContext(_jobQueueManagerSettings.ConnectionString))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                entity.Status = (short)status;
                saved = SaveChanges(entity, context);
            }

            return saved;
        }

        public bool SaveChanges(JobEntity jobEntity, JobQueueDataContext dataContext)
        {
            var saved = false;

            dataContext.Entry(jobEntity).State = EntityState.Modified;
            jobEntity.DateTimeUpdatedUtc = DateTime.UtcNow;

            try
            {
                dataContext.SaveChanges();
                saved = true;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw;
            }
            catch (Exception ex)
            {
                // log ??
                saved = false;
            }

            return saved;
        }
    }
}
