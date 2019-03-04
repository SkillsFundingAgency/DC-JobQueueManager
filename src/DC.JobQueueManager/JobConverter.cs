using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager
{
    public static class JobConverter
    {
        public static JobQueueManager.Data.Entities.Job Convert(Jobs.Model.Job source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new JobQueueManager.Data.Entities.Job();
            Convert(source, entity);
            return entity;
        }

        public static Jobs.Model.Job Convert(JobQueueManager.Data.Entities.Job source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new Jobs.Model.Job();
            Convert(source, entity);
            return entity;
        }

        public static void Convert(Jobs.Model.Job source, JobQueueManager.Data.Entities.Job destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.JobType = (short)source.JobType;
            destination.Priority = source.Priority;
            destination.Status = (short)source.Status;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.SubmittedBy = source.SubmittedBy;
            destination.NotifyEmail = source.NotifyEmail;
            destination.RowVersion = source.RowVersion == null ? null : System.Text.Encoding.UTF8.GetBytes(source.RowVersion);
            destination.CrossLoadingStatus = source.CrossLoadingStatus.HasValue ? (short)source.CrossLoadingStatus : (short?)null;
        }

        public static void Convert(JobQueueManager.Data.Entities.Job source, Jobs.Model.Job destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.Priority = source.Priority;
            destination.Status = (JobStatus.Interface.JobStatusType)source.Status;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.ToBase64String(source.RowVersion);
            destination.SubmittedBy = source.SubmittedBy;
            destination.NotifyEmail = source.NotifyEmail;
            destination.JobType = (Jobs.Model.Enums.JobType)source.JobType;
            destination.CrossLoadingStatus = source.CrossLoadingStatus.HasValue ? (JobStatus.Interface.JobStatusType)source.CrossLoadingStatus.Value : (JobStatus.Interface.JobStatusType?)null;
        }

        public static void Convert(JobQueueManager.Data.Entities.Job source, FileUploadJob destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.Priority = source.Priority;
            destination.Status = (JobStatus.Interface.JobStatusType)source.Status;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.ToBase64String(source.RowVersion);
            destination.SubmittedBy = source.SubmittedBy;
            destination.NotifyEmail = source.NotifyEmail;
            destination.JobType = (Jobs.Model.Enums.JobType)source.JobType;
            destination.CrossLoadingStatus = source.CrossLoadingStatus.HasValue ? (JobStatus.Interface.JobStatusType)source.CrossLoadingStatus.Value : (JobStatus.Interface.JobStatusType?)null;
        }

        public static void Convert(FileUploadJobMetaData source, FileUploadJob destination)
        {
            if (source == null)
            {
                return;
            }

            if (destination == null)
            {
                destination = new FileUploadJob();
            }

            destination.FileName = source.FileName;
            destination.FileSize = source.FileSize.GetValueOrDefault(0);
            destination.StorageReference = source.StorageReference;
            destination.JobId = source.JobId;
            destination.IsFirstStage = source.IsFirstStage;
            destination.CollectionName = source.CollectionName;
            destination.PeriodNumber = source.PeriodNumber;
            destination.Ukprn = source.Ukprn;
            destination.TermsAccepted = source.TermsAccepted;
            destination.CollectionYear = source.CollectionYear;
            Convert(source.Job, destination);
        }

        public static void Convert(FileUploadJob source, FileUploadJobMetaData destination)
        {
            if (source == null)
            {
                return;
            }

            if (destination == null)
            {
                destination = new FileUploadJobMetaData();
            }

            destination.FileName = source.FileName;
            destination.FileSize = source.FileSize;
            destination.StorageReference = source.StorageReference;
            destination.JobId = source.JobId;
            destination.IsFirstStage = source.IsFirstStage;
            destination.CollectionName = source.CollectionName;
            destination.PeriodNumber = source.PeriodNumber;
            destination.Ukprn = source.Ukprn;
            destination.TermsAccepted = source.TermsAccepted;
            destination.CollectionYear = source.CollectionYear;
        }
    }
}