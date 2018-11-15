using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class Job
    {
        public Job()
        {
            FileUploadJobMetaData = new HashSet<FileUploadJobMetaData>();
        }

        public long JobId { get; set; }
        public short JobType { get; set; }
        public short Priority { get; set; }
        public DateTime DateTimeSubmittedUtc { get; set; }
        public DateTime? DateTimeUpdatedUtc { get; set; }
        public string SubmittedBy { get; set; }
        public short Status { get; set; }
        public byte[] RowVersion { get; set; }
        public string NotifyEmail { get; set; }
        public short? CrossLoadingStatus { get; set; }

        public ICollection<FileUploadJobMetaData> FileUploadJobMetaData { get; set; }
    }
}
