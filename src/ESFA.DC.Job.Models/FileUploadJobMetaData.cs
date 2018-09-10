namespace ESFA.DC.Job.Models
{
    public class FileUploadJobMetaData
    {
        public string StorageReference { get; set; }

        public string FileName { get; set; }

        public decimal FileSize { get; set; }

        public bool IsFirstStage { get; set; }

        public long JobId { get; set; }

        public int PeriodNumber { get; set; }

        public string CollectionName { get; set; }

        public long Ukprn { get; set; }
    }
}