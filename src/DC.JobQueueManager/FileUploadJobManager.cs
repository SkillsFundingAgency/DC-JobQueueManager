using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Func<IJobQueueDataContext> _contextFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailNotifier _emailNotifier;
        private readonly ILogger _logger;

        public FileUploadJobManager(
            Func<IJobQueueDataContext> contextFactory,
            IDateTimeProvider dateTimeProvider,
            IReturnCalendarService returnCalendarService,
            IEmailTemplateManager emailTemplateManager,
            IEmailNotifier emailNotifier,
            ILogger logger)
        : base(contextFactory, returnCalendarService, emailTemplateManager)
        {
            _contextFactory = contextFactory;
            _dateTimeProvider = dateTimeProvider;
            _emailNotifier = emailNotifier;
            _logger = logger;
        }

        public async Task<FileUploadJob> GetJobById(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = _contextFactory())
            {
                var entity = await context.FileUploadJobMetaData.Include(x => x.Job).SingleOrDefaultAsync(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var job = new FileUploadJob();
                JobConverter.Convert(entity, job);
                return job;
            }
        }

        public async Task<long> AddJob(FileUploadJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = _contextFactory())
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
                await context.SaveChangesAsync();

                //send email on create for esf and eas
                if (job.JobType != JobType.IlrSubmission)
                {
                    await SendEmailNotification(entity.JobId);
                }

                return entity.JobId;
            }
        }

        public async Task<bool> UpdateJobStage(long jobId, bool isFirstStage)
        {
            using (var context = _contextFactory())
            {
                var entity = await context.FileUploadJobMetaData.SingleOrDefaultAsync(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                entity.IsFirstStage = isFirstStage;
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    await context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    throw new Exception("Save failed. Job details have been changed. Reload the job object and try save again");
                }
            }
        }

        public async Task<IEnumerable<FileUploadJob>> GetJobsByUkprn(long ukprn)
        {
            using (var context = _contextFactory())
            {
                var entities = await context.FileUploadJobMetaData.Include(x => x.Job).Where(x => x.Ukprn == ukprn)
                    .ToListAsync();
                return ConvertJobs(entities);
            }
        }

        public async Task<IEnumerable<FileUploadJob>> GetJobsByUkprnForDateRange(long ukprn, DateTime startDateTimeUtc,
            DateTime endDateTimeUtc)
        {
            using (var context = _contextFactory())
            {
                var entities = await context.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .Where(x => x.Ukprn == ukprn &&
                                x.Job.DateTimeSubmittedUtc >= startDateTimeUtc &&
                                x.Job.DateTimeSubmittedUtc <= endDateTimeUtc)
                    .ToListAsync();
                return ConvertJobs(entities);
            }
        }

        public async Task<IEnumerable<FileUploadJob>> GetJobsByUkprnForPeriod(long ukprn, int period)
        {
            using (var context = _contextFactory())
            {
                var entities = await context.FileUploadJobMetaData.Include(x => x.Job)
                    .Where(x => x.Ukprn == ukprn && x.PeriodNumber == period)
                    .ToListAsync();
                return ConvertJobs(entities);
            }
        }

        public async Task<IEnumerable<FileUploadJob>> GetLatestJobsPerPeriodByUkprn(long ukprn,
            DateTime startDateTimeUtc, DateTime endDateTimeUtc)
        {
            using (var context = _contextFactory())
            {
                var entities = await context.FileUploadJobMetaData.Include(x => x.Job)
                    .Where(x => x.Ukprn == ukprn && 
                                x.Job.Status == (short)JobStatusType.Completed &&
                                x.Job.DateTimeSubmittedUtc >= startDateTimeUtc &&
                                x.Job.DateTimeSubmittedUtc <= endDateTimeUtc)
                    .GroupBy(x => new { x.CollectionYear, x.PeriodNumber, x.Job.JobType})
                    .Select(g => g.OrderByDescending(x => x.Job.DateTimeSubmittedUtc).FirstOrDefault())
                    .ToListAsync();
                return ConvertJobs(entities);
            }
        }

        public FileUploadJob GetLatestJobByUkprn(long ukprn, string collectionName)
        {
            var result = new FileUploadJob();
            using (var context = _contextFactory())
            {
                var entity = context.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .Where(x => x.Ukprn == ukprn && x.CollectionName.Equals(collectionName, StringComparison.CurrentCultureIgnoreCase))
                    .OrderByDescending(x => x.Job.DateTimeSubmittedUtc)
                    .FirstOrDefault();

                JobConverter.Convert(entity, result);
            }

            return result;
        }

        public async Task<IEnumerable<FileUploadJob>> GetLatestJobByUkprn(long[] ukprns)
        {
            using (var context = _contextFactory())
            {
                var entities = await context.FileUploadJobMetaData
                    .Join(context.Job,
                        metadata => metadata.JobId,
                        job => job.JobId,
                        (metadata,job) => new { metadata, job})
                    .Where(x => ukprns.Contains(x.metadata.Ukprn))
                    .GroupBy(x => x.metadata.Ukprn)
                    .Select(g => g.OrderByDescending(x => x.job.DateTimeSubmittedUtc).FirstOrDefault())
                    .ToListAsync();

                entities.ForEach(x => x.metadata.Job = x.job);

                return ConvertJobs(entities.Select(x => x.metadata));
            }
        }

        public async Task<FileUploadJob> GetLatestJobByUkprnAndContractReference(long ukprn, string contractReference,
            string collectionName)
        {
            var result = new FileUploadJob();
            var fileNameSearchQuery = $"{ukprn}/SUPPDATA-{ukprn}-{contractReference}-";
            using (var context = _contextFactory())
            {
                var entity = await context.FileUploadJobMetaData
                    .Include(x => x.Job)
                    .Where(
                        x => x.Ukprn == ukprn &&
                             x.CollectionName.Equals(collectionName, StringComparison.CurrentCultureIgnoreCase) &&
                             x.FileName.StartsWith(fileNameSearchQuery))
                    .OrderByDescending(x => x.Job.DateTimeSubmittedUtc)
                    .FirstOrDefaultAsync();

                JobConverter.Convert(entity, result);
            }

            return result;
        }


        public async Task<IEnumerable<FileUploadJob>> GetAllJobs()
        {
            using (var context = _contextFactory())
            {
                var entities = await context.FileUploadJobMetaData.Include(x => x.Job).ToListAsync();
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

        public async Task SendEmailNotification(long jobId)
        {
            try
            {
                var job = await GetJobById(jobId);
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