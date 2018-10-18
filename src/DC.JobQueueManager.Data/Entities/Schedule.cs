using System;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class Schedule
    {
        [Key]
        public long ID { get; set; }

        [Required] public bool Enabled { get; set; }

        public bool? MinuteIsCadence { get; set; }

        public byte? Minute { get; set; }

        public byte? Hour { get; set; }

        public byte? DayOfTheMonth { get; set; }

        public byte? Month { get; set; }

        public byte? DayOfTheWeek { get; set; }

        [Required] public string ExternalDataType { get; set; }

        [Required] public bool ExecuteOnceOnly { get; set; }

        public DateTime? LastExecuteDateTime { get; set; }
    }
}
