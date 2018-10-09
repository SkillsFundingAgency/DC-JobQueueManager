using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
            : base(contextOptions, returnCalendarService, dateTimeProvider)
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
                    CrossLoadingStatus = IsCrossLoadingEnabled(job.JobType) ? (short)JobStatusType.Ready : (short?)null
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
                var jobEntity = context.Jobs.FromSql("GetJobByPriority").FirstOrDefault();
                if (jobEntity == null)
                {
                    return null;
                }

                var job = JobConverter.Convert(jobEntity);
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

                var statusChanged = false;

                if (entity.CrossLoadingStatus.HasValue && entity.CrossLoadingStatus != (short?)job.CrossLoadingStatus)
                {
                    statusChanged = true;
                }
                else if (entity.Status != (short)job.Status)
                {
                    statusChanged = true;
                }

                JobConverter.Convert(job, entity);
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).Property("RowVersion").OriginalValue = Convert.FromBase64String(job.RowVersion);
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();

                    if (statusChanged)
                    {
                        SendEmailNotification(job.JobId, job.CrossLoadingStatus ?? job.Status, job.JobType);
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

                var statusChanged = !entity.CrossLoadingStatus.HasValue && entity.Status != (short)status;

                entity.Status = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();

                if (statusChanged)
                {
                    SendEmailNotification(jobId, status, (JobType)entity.JobType);
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

                var statusChanged = entity.CrossLoadingStatus.HasValue && entity.CrossLoadingStatus != (short)status;

                entity.CrossLoadingStatus = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();

                if (statusChanged)
                {
                    SendEmailNotification(jobId, status, (JobType)entity.JobType, entity.DateTimeSubmittedUtc);
                }

                return true;
            }
        }

        public void SendEmailNotification(long jobId, JobStatusType status, JobType jobType, DateTime dateTimeJobSubmittedUtc)
        {
            try
            {
                var template = _emailTemplateManager.GetTemplate(jobId, status, jobType, dateTimeJobSubmittedUtc);

                if (!string.IsNullOrEmpty(template))
                {
                    var personalisation = new Dictionary<string, dynamic>();
                    var job = GetJobById(jobId);

                    PopulatePersonalisation(jobId, personalisation);

                    if (jobType == JobType.IlrSubmission || jobType == JobType.EsfSubmission ||
                        jobType == JobType.EasSubmission)
                    {
                        _fileUploadJobManager.PopulatePersonalisation(jobId, personalisation);
                    }

                    _emailNotifier.SendEmail(job.NotifyEmail, template, personalisation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sending email failed for job {jobId}", ex);
            }
        }

        public void PopulatePersonalisation(long jobId, Dictionary<string, dynamic> personalisation)
        {
            var job = GetJobById(jobId);
            var submittedAt = _dateTimeProvider.ConvertUtcToUk(job.DateTimeSubmittedUtc);
            personalisation.Add("JobId", job.JobId);
            personalisation.Add("Name", job.SubmittedBy);
            personalisation.Add(
                "DateTimeSubmitted",
                string.Concat(submittedAt.ToString("hh:mm tt"), " on ", submittedAt.ToString("dddd dd MMMM yyyy")));
        }
    }
}