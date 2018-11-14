using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public sealed class JobManager : AbstractJobManager, IJobManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailNotifier _emailNotifier;
        private readonly IFileUploadJobManager _fileUploadJobManager;
        private readonly IEmailTemplateManager _emailTemplateManager;
        private readonly ILogger _logger;

        public JobManager(
            DbContextOptions contextOptions,
            IDateTimeProvider dateTimeProvider,
            IEmailNotifier emailNotifier,
            IFileUploadJobManager fileUploadJobManager,
            IEmailTemplateManager emailTemplateManager,
            ILogger logger,
            IReturnCalendarService returnCalendarService)
            : base(contextOptions, returnCalendarService, emailTemplateManager)
        {
            _contextOptions = contextOptions;
            _dateTimeProvider = dateTimeProvider;
            _emailNotifier = emailNotifier;
            _fileUploadJobManager = fileUploadJobManager;
            _emailTemplateManager = emailTemplateManager;
            _logger = logger;
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
                    SubmittedBy = job.SubmittedBy,
                    NotifyEmail = job.NotifyEmail,
                    CrossLoadingStatus =
                        IsCrossLoadingEnabled(job.JobType) ? (short)JobStatusType.Ready : (short?)null
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

        public async Task<IEnumerable<Job>> GetJobsByPriorityAsync()
        {
            List<Job> jobs = new List<Job>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                JobEntity[] jobEntities = await context.Jobs.FromSql("GetJobByPriority").ToArrayAsync();

                foreach (JobEntity jobEntity in jobEntities)
                {
                    jobs.Add(JobConverter.Convert(jobEntity));
                }
            }

            return jobs;
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

                bool statusChanged = entity.Status != (short)job.Status;

                JobConverter.Convert(job, entity);
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).Property("RowVersion").OriginalValue =
                    job.RowVersion == null ? null : Convert.FromBase64String(job.RowVersion);
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();

                    if (statusChanged)
                    {
                        SendEmailNotification(job);
                    }

                    return true;
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    throw new Exception(
                        "Save failed. Job details have been changed. Reload the job object and try save again");
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

                var statusChanged = entity.Status != (short)status;

                entity.Status = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();

                if (statusChanged)
                {
                    SendEmailNotification(jobId);
                }

                return true;
            }
        }

        public bool UpdateCrossLoadingStatus(long jobId, JobStatusType status)
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

                entity.CrossLoadingStatus = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();

                return true;
            }
        }

        public void SendEmailNotification(Job job)
        {
            try
            {
                var template =
                    _emailTemplateManager.GetTemplate(job.JobId, job.Status, job.JobType, job.DateTimeSubmittedUtc);

                if (!string.IsNullOrEmpty(template))
                {
                    var personalisation = new Dictionary<string, dynamic>();

                    var submittedAt = _dateTimeProvider.ConvertUtcToUk(job.DateTimeSubmittedUtc);
                    personalisation.Add("JobId", job.JobId);
                    personalisation.Add("Name", job.SubmittedBy);
                    personalisation.Add("DateTimeSubmitted", string.Concat(submittedAt.ToString("hh:mm tt"), " on ", submittedAt.ToString("dddd dd MMMM yyyy")));

                    _emailNotifier.SendEmail(job.NotifyEmail, template, personalisation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sending email failed for job {job.JobId}", ex);
            }
        }

        public void SendEmailNotification(long jobId)
        {
            var job = GetJobById(jobId);
            SendEmailNotification(job);
        }
    }
}