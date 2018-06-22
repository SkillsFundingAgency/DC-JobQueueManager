using System;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager
{
    public static class JobConverter
    {
        public static JobEntity Convert(Job source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new JobEntity();
            Convert(source, entity);
            return entity;
        }

        public static Job Convert(JobEntity source)
        {
            if (source == null)
            {
                return null;
            }

            var entity = new Job();
            Convert(source, entity);
            return entity;
        }

        public static void Convert(Job source, JobEntity destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.JobType = (short)source.JobType;
            destination.Priority = source.Priority;
            destination.Status = (short)source.Status;
            destination.Ukprn = source.Ukprn;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.SubmittedBy = source.SubmittedBy;
        }

        public static void Convert(JobEntity source, Job destination)
        {
            destination.DateTimeSubmittedUtc = source.DateTimeSubmittedUtc;
            destination.JobType = (JobType)source.JobType;
            destination.Priority = source.Priority;
            destination.Status = (JobStatusType)source.Status;
            destination.Ukprn = source.Ukprn;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.ToBase64String(source.RowVersion);
            destination.SubmittedBy = source.SubmittedBy;
        }

        public static void Convert(IlrJobMetaDataEntity source, IlrJobMetaData destination)
        {
            if (source == null)
            {
                return;
            }

            if (destination == null)
            {
                destination = new IlrJobMetaData();
            }

            destination.FileName = source.FileName;
            destination.FileSize = source.FileSize;
            destination.StorageReference = source.StorageReference;
            destination.JobId = source.JobId;
            destination.TotalLearners = source.TotalLearners;
            destination.IsFirstStage = source.IsFirstStage;
        }

        public static void Convert(IlrJobMetaData source, IlrJobMetaDataEntity destination)
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
        }
    }
}