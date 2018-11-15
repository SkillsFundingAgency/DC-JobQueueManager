using System.ComponentModel.DataAnnotations;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class CollectionTypeEntity
    {
        [Key]
        public int CollectionTypeId { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }
    }
}
