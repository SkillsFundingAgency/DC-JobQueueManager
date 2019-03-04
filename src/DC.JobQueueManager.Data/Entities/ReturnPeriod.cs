using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class ReturnPeriod
    {
        public int ReturnPeriodId { get; set; }
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public int PeriodNumber { get; set; }
        public int CollectionId { get; set; }
        public int CalendarMonth { get; set; }
        public int CalendarYear { get; set; }

        public Collection Collection { get; set; }
    }
}
