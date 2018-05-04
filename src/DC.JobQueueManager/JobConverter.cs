using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using DC.JobQueueManager.Data.Entities;
using DC.JobQueueManager.Models;
using DC.JobQueueManager.Models.Enums;

namespace DC.JobQueueManager
{
    public static class JobConverter
    {
        public static JobEntity Convert(Job source)
        {
            var entity = new JobEntity();
            Convert(source, entity);
            return entity;
        }

        public static Job Convert(JobEntity source)
        {
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
            destination.RowVersion = source.RowVersion;
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
            destination.RowVersion = source.RowVersion;
        }
    }
}
