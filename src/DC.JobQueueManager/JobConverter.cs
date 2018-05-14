using System;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Models;
using ESFA.DC.JobQueueManager.Models.Enums;

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
            destination.DateTimeSubmittedUtc = DateTime.UtcNow;
            destination.FileName = source.FileName;
            destination.JobType = (short)source.JobType;
            destination.Priority = source.Priority;
            destination.Status = (short)source.Status;
            destination.StorageReference = source.StorageReference;
            destination.Ukprn = source.Ukprn;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.FromBase64String(source.RowVersion);
        }

        public static void Convert(JobEntity source, Job destination)
        {
            destination.DateTimeSubmittedUtc = DateTime.UtcNow;
            destination.FileName = source.FileName;
            destination.JobType = (JobType)source.JobType;
            destination.Priority = source.Priority;
            destination.Status = (JobStatus)source.Status;
            destination.StorageReference = source.StorageReference;
            destination.Ukprn = source.Ukprn;
            destination.DateTimeUpdatedUtc = source.DateTimeUpdatedUtc;
            destination.JobId = source.JobId;
            destination.RowVersion = source.RowVersion == null ? null : System.Convert.ToBase64String(source.RowVersion);
        }
    }
}