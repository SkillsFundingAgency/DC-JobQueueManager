using System;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager
{
    public static class IlrJobConverter
    {
        public static JobEntity Convert(IlrJob source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new JobEntity();
            Convert(source, entity);
            return entity;
        }

        public static IlrJob Convert(JobEntity source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new IlrJob();
            Convert(source, entity);
            return entity;
        }

        public static void Convert(IlrJob source, JobEntity destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.JobType = (short)source.JobType;
            destination.Priority = source.Priority;
            destination.Status = (short)source.Status;
            destination.Ukprn = source.Ukprn;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.SubmittedBy = source.SubmittedBy;
            destination.NotifyEmail = source.NotifyEmail;
            destination.RowVersion = System.Text.Encoding.UTF8.GetBytes(source.RowVersion);
        }

        public static void Convert(JobEntity source, IlrJob destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.Priority = source.Priority;
            destination.Status = (JobStatusType)source.Status;
            destination.Ukprn = source.Ukprn;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.ToBase64String(source.RowVersion);
            destination.SubmittedBy = source.SubmittedBy;
            destination.NotifyEmail = source.NotifyEmail;
        }

        public static void Convert(IlrJobMetaDataEntity source, IlrJob destination)
        {
            if (source == null)
            {
                return;
            }

            if (destination == null)
            {
                destination = new IlrJob();
            }

            destination.FileName = source.FileName;
            destination.FileSize = source.FileSize;
            destination.StorageReference = source.StorageReference;
            destination.JobId = source.JobId;
            destination.TotalLearners = source.TotalLearners;
            destination.IsFirstStage = source.IsFirstStage;
            destination.CollectionName = source.CollectionName;
            destination.PeriodNumber = source.PeriodNumber;
        }

        public static void Convert(IlrJob source, IlrJobMetaDataEntity destination)
        {
            if (source == null)
            {
                return;
            }

            if (destination == null)
            {
                destination = new IlrJobMetaDataEntity();
            }

            destination.FileName = source.FileName;
            destination.FileSize = source.FileSize;
            destination.StorageReference = source.StorageReference;
            destination.JobId = source.JobId;
            destination.TotalLearners = source.TotalLearners;
            destination.IsFirstStage = source.IsFirstStage;
            destination.CollectionName = source.CollectionName;
            destination.PeriodNumber = source.PeriodNumber;
        }
    }
}