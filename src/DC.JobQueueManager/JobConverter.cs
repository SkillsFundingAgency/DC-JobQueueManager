using System;
using ESFA.DC.Job.Models;
using ESFA.DC.Job.Models.Enums;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager
{
    public static class JobConverter
    {
        public static JobEntity Convert(Job.Models.Job source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new JobEntity();
            Convert(source, entity);
            return entity;
        }

        public static Job.Models.Job Convert(JobEntity source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new Job.Models.Job();
            Convert(source, entity);
            return entity;
        }

        public static void Convert(Job.Models.Job source, JobEntity destination)
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
        }

        public static void Convert(JobEntity source, Job.Models.Job destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.Priority = source.Priority;
            destination.Status = (JobStatusType)source.Status;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.ToBase64String(source.RowVersion);
            destination.SubmittedBy = source.SubmittedBy;
            destination.NotifyEmail = source.NotifyEmail;
            destination.JobType = (JobType)source.JobType;
        }

        public static void Convert(FileUploadJobMetaDataEntity source, FileUploadJobMetaData destination)
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
        }

        public static void Convert(FileUploadJobMetaData source, FileUploadJobMetaDataEntity destination)
        {
            if (source == null)
            {
                return;
            }

            if (destination == null)
            {
                destination = new FileUploadJobMetaDataEntity();
            }

            destination.FileName = source.FileName;
            destination.FileSize = source.FileSize;
            destination.StorageReference = source.StorageReference;
            destination.JobId = source.JobId;
            destination.IsFirstStage = source.IsFirstStage;
            destination.CollectionName = source.CollectionName;
            destination.PeriodNumber = source.PeriodNumber;
            destination.Ukprn = source.Ukprn;
        }
    }
}