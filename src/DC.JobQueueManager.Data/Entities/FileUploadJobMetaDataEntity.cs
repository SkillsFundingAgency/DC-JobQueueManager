﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class FileUploadJobMetaDataEntity
    {
        [Key]
        public long Id { get; set; }

        public string StorageReference { get; set; }

        [Required]
        public string FileName { get; set; }

        public decimal FileSize { get; set; }

        public bool IsFirstStage { get; set; }

        [ForeignKey("JobId")]
        public JobEntity Job { get; set; }

        [Required]
        public long JobId { get; set; }

        public int PeriodNumber { get; set; }

        public string CollectionName { get; set; }

        public long Ukprn { get; set; }

        public bool? TermsAccepted { get; set; }
    }
}