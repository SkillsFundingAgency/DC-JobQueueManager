using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class JobSubmission
    {
        [Key]
        public long Id { get; set; }

        public long JobId { get; set; }

        public DateTime DateTimeUtc { get; set; }

        public Job Job { get; set; }
    }
}
