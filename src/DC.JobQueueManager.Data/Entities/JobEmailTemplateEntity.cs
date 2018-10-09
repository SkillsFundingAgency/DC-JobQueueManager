using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class JobEmailTemplateEntity
    {
        public string TemplateOpenPeriod { get; set; }

        public string TemplateClosePeriod { get; set; }

        public short JobStatus { get; set; }

        public bool Active { get; set; }

        public short JobType { get; set; }
    }
}
