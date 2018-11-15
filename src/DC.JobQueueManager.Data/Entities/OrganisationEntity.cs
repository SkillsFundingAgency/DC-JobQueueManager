using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class OrganisationEntity
    {
        public OrganisationEntity()
        {
            OrganisationCollection = new HashSet<OrganisationCollectionEntity>();
        }

        [Key]
        public int OrganisationId { get; set; }

        public string OrgId { get; set; }

        public long Ukprn { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public ICollection<OrganisationCollectionEntity> OrganisationCollection { get; set; }
    }
}
