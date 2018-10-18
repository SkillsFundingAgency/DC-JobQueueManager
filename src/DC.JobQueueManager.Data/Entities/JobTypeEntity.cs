using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class JobTypeEntity
    {
        [Key]
        public int JobTypeId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsCrossLoadingEnabled { get; set; }
    }
}
