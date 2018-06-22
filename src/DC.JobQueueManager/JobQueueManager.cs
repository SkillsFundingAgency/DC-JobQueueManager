using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.DateTime.Provider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ESFA.DC.JobQueueManager
{
    public sealed class JobQueueManager : IJobQueueManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IDateTimeProvider _dateTimeProvider;

        public JobQueueManager(DbContextOptions contextOptions, IDateTimeProvider dateTimeProvider)
        {
            _contextOptions = contextOptions;
            _dateTimeProvider = dateTimeProvider;
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
                    DateTimeSubmittedUtc = _dateTimeProvider.GetNowUtc(),
                    JobType = (short)job.JobType,
                    Priority = job.Priority,
                    Status = (short)job.Status,
                    Ukprn = job.Ukprn,
                    SubmittedBy = job.SubmittedBy
                };
                //TODO: May be review and re-arrange this area when more job types become relevant
                if (job.JobType == JobType.IlrSubmission)
                {
                    var metaEntity = new IlrJobMetaDataEntity()
                    {
                        FileName = ((IlrJobMetaData)job.JobMetaData).FileName,
                        FileSize = ((IlrJobMetaData)job.JobMetaData).FileSize,
                        IsFirstStage = true,
                        StorageReference = ((IlrJobMetaData)job.JobMetaData).StorageReference,
                        Job = entity
                    };
                    context.IlrJobMetaDataEntities.Add(metaEntity);
                }

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
                LoadIlrJobMetaData(jobs, context);
            }

            return jobs;
        }

        public IEnumerable<Job> GetJobsByUkprn(long ukprn)
        {
            if (ukprn == 0)
            {
                throw new ArgumentException("ukprn can not be 0");
            }

            var jobs = new List<Job>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntities = context.Jobs.Where(x => x.Ukprn == ukprn).ToList();
                jobEntities.ForEach(x =>
                    jobs.Add(JobConverter.Convert(x)));
                LoadIlrJobMetaData(jobs, context);
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
                LoadIlrJobMetaData(job, context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId));
                return job;
            }
        }

        public Job GetJobByPriority()
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntity = context.Jobs.FromSql("GetJobByPriority").FirstOrDefault();
                var job = JobConverter.Convert(jobEntity);
                LoadIlrJobMetaData(job, context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == job.JobId));
                return job;
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
                var jobEntity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (jobEntity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                if (jobEntity.Status != 1) // if already moved, then dont delete
                {
                    throw new ArgumentOutOfRangeException("Job is already moved from ready status, unable to delete");
                }

                var metaDataEntity = context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId);
                if (metaDataEntity != null)
                {
                    context.IlrJobMetaDataEntities.Remove(metaDataEntity);
                }

                context.Jobs.Remove(jobEntity);
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
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
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

        public bool UpdateJobMetaData(IlrJobMetaData jobMetaData)
        {
            if (jobMetaData == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobMetaData.JobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobMetaData.JobId} does not exist");
                }

                JobConverter.Convert(jobMetaData, entity);
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

        public bool UpdateJobStatus(long jobId, JobStatusType status)
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

                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();
                return true;
            }
        }

        public void LoadIlrJobMetaData(IEnumerable<Job> jobs, JobQueueDataContext context)
        {
            foreach (var job in jobs.Where(x => x.JobType == JobType.IlrSubmission))
            {
                LoadIlrJobMetaData(job, context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == job.JobId));
            }
        }

        public void LoadIlrJobMetaData(Job job, IlrJobMetaDataEntity metaDataEntity)
        {
            var metaData = new IlrJobMetaData();
            JobConverter.Convert(metaDataEntity, metaData);
            job.JobMetaData = metaData;
        }
    }
}