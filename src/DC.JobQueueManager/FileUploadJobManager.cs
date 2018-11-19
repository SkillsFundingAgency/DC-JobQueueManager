using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Job = ESFA.DC.JobQueueManager.Data.Entities.Job;
using JobStatusType = ESFA.DC.JobStatus.Interface.JobStatusType;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;

namespace ESFA.DC.JobQueueManager
{
    public sealed class FileUploadJobManager : AbstractJobManager, IFileUploadJobManager
    {
        private readonly DbContextOptions<JobQueueDataContext> _contextOptions;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailNotifier _emailNotifier;
        private readonly ILogger _logger;

        public FileUploadJobManager(
            DbContextOptions<JobQueueDataContext> contextOptions,
            IDateTimeProvider dateTimeProvider,
            IReturnCalendarService returnCalendarService,
            IEmailTemplateManager emailTemplateManager,
            IEmailNotifier emailNotifier,
            ILogger logger)
        : base(contextOptions, returnCalendarService, emailTemplateManager)
        {
            _contextOptions = contextOptions;
            _dateTimeProvider = dateTimeProvider;
            _emailNotifier = emailNotifier;
            _logger = logger;
        }

        public FileUploadJob GetJobById(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.FileUploadJobMetaData.Include(x => x.Job).SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var job = new FileUploadJob();
                JobConverter.Convert(entity, job);
                return job;
            }
        }

        public long AddJob(FileUploadJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = new Job
                {
                    DateTimeSubmittedUtc = _dateTimeProvider.GetNowUtc(),
                    JobType = (short)job.JobType,
                    Priority = job.Priority,
                    Status = (short)job.Status,
                    SubmittedBy = job.SubmittedBy,
                    NotifyEmail = job.NotifyEmail,
                    CrossLoadingStatus = IsCrossLoadingEnabled(job.JobType) ? (short)JobStatusType.Ready : (short?)null
                };

                var metaEntity = new FileUploadJobMetaData()
                {
                    FileName = job.FileName,
                    FileSize = job.FileSize,
                    IsFirstStage = true,
                    StorageReference = job.StorageReference,
                    Job = entity,
                    CollectionName = job.CollectionName,
                    PeriodNumber = job.PeriodNumber,
                    Ukprn = job.Ukprn,
                    TermsAccepted = job.TermsAccepted,
                    CollectionYear = job.CollectionYear
                };
                context.Job.Add(entity);
                context.FileUploadJobMetaData.Add(metaEntity);
                context.SaveChanges();

                //send email on create for esf and eas
                if (job.JobType != JobType.IlrSubmission)
                {
                    SendEmailNotification(entity.JobId);
                }

                return entity.JobId;
            }
        }

        public bool UpdateJobStage(long jobId, bool isFirstStage)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.FileUploadJobMetaData.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                entity.IsFirstStage = isFirstStage;
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

        public IEnumerable<FileUploadJob> GetJobsByUkprn(long ukprn)
        {
            var items = new List<FileUploadJob>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entities = context.FileUploadJobMetaData.Include(x => x.Job).Where(x => x.Ukprn == ukprn)
                    .ToList();
                return ConvertJobs(entities);
            }
        }

        public IEnumerable<FileUploadJob> GetJobsByUkprnForDateRange(long ukprn, DateTime startDateTimeUtc, DateTime endDateTimeUtc)
        {
            var items = new List<FileUploadJob>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entities = context.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .Where(x => x.Ukprn == ukprn &&
                                x.Job.DateTimeSubmittedUtc >= startDateTimeUtc &&
                                x.Job.DateTimeSubmittedUtc <= endDateTimeUtc)
                    .ToList();
                return ConvertJobs(entities);
            }
        }

        public IEnumerable<FileUploadJob> GetJobsByUkprnForPeriod(long ukprn, int period)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entities = context.FileUploadJobMetaData.Include(x => x.Job)
                    .Where(x => x.Ukprn == ukprn && x.PeriodNumber == period)
                    .ToList();
                return ConvertJobs(entities);
            }
        }

        public FileUploadJob GetLatestJobByUkprn(long ukprn)
        {
            var result = new FileUploadJob();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .OrderByDescending(x => x.Job.DateTimeSubmittedUtc)
                    .FirstOrDefault();

                JobConverter.Convert(entity, result);
            }

            return result;
        }

        public IEnumerable<FileUploadJob> GetAllJobs()
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entities = context.FileUploadJobMetaData.Include(x => x.Job).ToList();
                return ConvertJobs(entities);
            }
        }

        public IEnumerable<FileUploadJob> ConvertJobs(IEnumerable<FileUploadJobMetaData> entities)
        {
            var items = new List<FileUploadJob>();
            foreach (var entity in entities)
            {
                var model = new FileUploadJob();
                JobConverter.Convert(entity, model);
                items.Add(model);
            }

            return items;
        }

        public void SendEmailNotification(long jobId)
        {
            try
            {
                var job = GetJobById(jobId);
                var template = GetTemplate(jobId, job.Status, job.JobType, job.DateTimeSubmittedUtc);

                if (!string.IsNullOrEmpty(template))
                {
                    var personalisation = new Dictionary<string, dynamic>();

                    var submittedAt = _dateTimeProvider.ConvertUtcToUk(job.DateTimeSubmittedUtc);

                    personalisation.Add("JobId", job.JobId);
                    personalisation.Add("Name", job.SubmittedBy);
                    personalisation.Add(
                        "DateTimeSubmitted",
                        string.Concat(submittedAt.ToString("hh:mm tt"), " on ", submittedAt.ToString("dddd dd MMMM yyyy")));

                    var nextReturnPeriod = GetNextReturnPeriod(job.CollectionName);
                    personalisation.Add("FileName", job.FileName);
                    personalisation.Add("CollectionName", job.CollectionName);
                    personalisation.Add(
                        "PeriodName",
                        $"R{job.PeriodNumber.ToString("00", NumberFormatInfo.InvariantInfo)}");
                    personalisation.Add("Ukprn", job.Ukprn);
                    if (nextReturnPeriod != null)
                    {
                        personalisation.Add("NextReturnOpenDate", nextReturnPeriod.StartDateTimeUtc.ToString("dd MMMM yyyy"));
                    }

                    _emailNotifier.SendEmail(job.NotifyEmail, template, personalisation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sending email failed for job {jobId}", ex);
            }
        }
    }
}