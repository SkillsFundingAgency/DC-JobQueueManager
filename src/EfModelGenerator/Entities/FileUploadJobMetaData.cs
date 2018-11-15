using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class FileUploadJobMetaData
    {
        public long Id { get; set; }
        public long JobId { get; set; }
        public string FileName { get; set; }
        public decimal? FileSize { get; set; }
        public string StorageReference { get; set; }
        public bool IsFirstStage { get; set; }
        public string CollectionName { get; set; }
        public int PeriodNumber { get; set; }
        public long Ukprn { get; set; }
        public bool? TermsAccepted { get; set; }

        public Job Job { get; set; }
    }
}
