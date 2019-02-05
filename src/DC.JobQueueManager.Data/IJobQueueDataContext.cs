using System;
using System.Threading;
using ESFA.DC.JobQueueManager.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager.Data
{
    public interface IJobQueueDataContext : IDisposable
    {
        DbSet<Collection> Collection { get; set; }
        DbSet<CollectionType> CollectionType { get; set; }
        DbSet<Eas> Eas { get; set; }
        DbSet<Efs> Efs { get; set; }
        DbSet<FileUploadJobMetaData> FileUploadJobMetaData { get; set; }
        DbSet<Ilr1819> Ilr1819 { get; set; }
        DbSet<Ilr1920> Ilr1920 { get; set; }
        DbSet<Job> Job { get; set; }
        DbSet<JobEmailTemplate> JobEmailTemplate { get; set; }
        DbSet<JobStatusType> JobStatusType { get; set; }
        DbSet<JobSubscriptionTask> JobSubscriptionTask { get; set; }
        DbSet<JobTopicSubscription> JobTopicSubscription { get; set; }
        DbSet<JobType> JobType { get; set; }
        DbSet<JobTypeGroup> JobTypeGroup { get; set; }
        DbSet<Organisation> Organisation { get; set; }
        DbSet<OrganisationCollection> OrganisationCollection { get; set; }
        DbSet<ReturnPeriod> ReturnPeriod { get; set; }
        DbSet<Schedule> Schedule { get; set; }
        int SaveChanges();
        System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry(object entity);
    }
}
