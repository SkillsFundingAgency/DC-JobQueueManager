namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class OrganisationCollectionEntity
    {
        public int OrganisationId { get; set; }

        public int CollectionId { get; set; }

        public CollectionEntity CollectionEntity { get; set; }

        public OrganisationEntity OrganisationEntity { get; set; }
    }
}
