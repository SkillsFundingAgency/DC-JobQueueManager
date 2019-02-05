using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Job = ESFA.DC.JobQueueManager.Data.Entities.Job;
using JobStatusType = ESFA.DC.JobStatus.Interface.JobStatusType;

namespace ESFA.DC.JobQueueManager
{
    public sealed class JobManager : AbstractJobManager, IJobManager
    {
        private readonly Func<IJobQueueDataContext> _contextFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailNotifier _emailNotifier;
        private readonly IFileUploadJobManager _fileUploadJobManager;
        private readonly IEmailTemplateManager _emailTemplateManager;
        private readonly ILogger _logger;

        public JobManager(
            Func<IJobQueueDataContext> contextFactory,
            IDateTimeProvider dateTimeProvider,
            IEmailNotifier emailNotifier,
            IFileUploadJobManager fileUploadJobManager,
            IEmailTemplateManager emailTemplateManager,
            ILogger logger,
            IReturnCalendarService returnCalendarService)
            : base(contextFactory, returnCalendarService, emailTemplateManager)
        {
            _contextFactory = contextFactory;
            _dateTimeProvider = dateTimeProvider;
            _emailNotifier = emailNotifier;
            _fileUploadJobManager = fileUploadJobManager;
            _emailTemplateManager = emailTemplateManager;
            _logger = logger;
        }

        public async Task<long> AddJob(Jobs.Model.Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (IJobQueueDataContext context = _contextFactory())
            {
                var entity = new Job
                {
                    DateTimeSubmittedUtc = _dateTimeProvider.GetNowUtc(),
                    JobType = (short)job.JobType,
                    Priority = job.Priority,
                    Status = (short)job.Status,
                    SubmittedBy = job.SubmittedBy,
                    NotifyEmail = job.NotifyEmail,
                    CrossLoadingStatus = (await IsCrossLoadingEnabled(job.JobType)) ? (short)JobStatusType.Ready : (short?)null
                };
                context.Job.Add(entity);
                context.SaveChanges();
                return entity.JobId;
            }
        }

        public async Task<IEnumerable<Jobs.Model.Job>> GetAllJobs()
        {
            var jobs = new List<Jobs.Model.Job>();
            using (IJobQueueDataContext context = _contextFactory())
            {
                var jobEntities = await context.Job.ToListAsync();
                jobEntities.ForEach(x => jobs.Add(JobConverter.Convert(x)));
            }

            return jobs;
        }

        public async Task<Jobs.Model.Job> GetJobById(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (IJobQueueDataContext context = _contextFactory())
            {
                var entity = await context.Job.SingleOrDefaultAsync(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var job = new Jobs.Model.Job();
                JobConverter.Convert(entity, job);
                return job;
            }
        }

        public async Task<IEnumerable<Jobs.Model.Job>> GetJobsByPriorityAsync(int resultCount)
        {
            List<Jobs.Model.Job> jobs = new List<Jobs.Model.Job>();
            using (IJobQueueDataContext context = _contextFactory())
            {
                Job[] jobEntities = await context.Job.FromSql("dbo.GetJobByPriority @ResultCount={0}", resultCount).ToArrayAsync();

                foreach (Job jobEntity in jobEntities)
                {
                    jobs.Add(JobConverter.Convert(jobEntity));
                }
            }

            return jobs;
        }

        public async Task RemoveJobFromQueue(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (IJobQueueDataContext context = _contextFactory())
            {
                var jobEntity = await context.Job.SingleOrDefaultAsync(x => x.JobId == jobId);
                if (jobEntity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                if (jobEntity.Status != 1) // if already moved, then dont delete
                {
                    throw new ArgumentOutOfRangeException("Job is already moved from ready status, unable to delete");
                }

                context.Job.Remove(jobEntity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateJob(Jobs.Model.Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (IJobQueueDataContext context = _contextFactory())
            {
                Job entity = await context.Job.SingleOrDefaultAsync(x => x.JobId == job.JobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {job.JobId} does not exist");
                }

                bool statusChanged = entity.Status != (short)job.Status;

                JobConverter.Convert(job, entity);
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).Property("RowVersion").OriginalValue = job.RowVersion == null ? null : Convert.FromBase64String(job.RowVersion);
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    await context.SaveChangesAsync();

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

        public async Task<bool> UpdateJobStatus(long jobId, JobStatusType status)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (IJobQueueDataContext context = _contextFactory())
            {
                var entity = await context.Job.SingleOrDefaultAsync(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var statusChanged = entity.Status != (short)status;

                entity.Status = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                await context.SaveChangesAsync();

                if (statusChanged)
                {
                    SendEmailNotification(jobId);
                }

                return true;
            }
        }

        public async Task<bool> UpdateCrossLoadingStatus(long jobId, JobStatusType status)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (IJobQueueDataContext context = _contextFactory())
            {
                var entity = await context.Job.SingleOrDefaultAsync(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                entity.CrossLoadingStatus = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                await context.SaveChangesAsync();

                return true;
            }
        }

        public void SendEmailNotification(Jobs.Model.Job job)
        {
            try
            {
                var template = _emailTemplateManager.GetTemplate(job.JobId, job.Status, job.JobType, job.DateTimeSubmittedUtc);

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

        public async Task SendEmailNotification(long jobId)
        {
            var job = await GetJobById(jobId);
            SendEmailNotification(job);
        }
    }
}