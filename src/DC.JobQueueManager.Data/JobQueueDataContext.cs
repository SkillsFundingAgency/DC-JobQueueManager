using ESFA.DC.JobQueueManager.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager.Data
{
    public sealed class JobQueueDataContext : DbContext
    {
        public JobQueueDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<JobEntity> Jobs { get; set; }

        public DbSet<FileUploadJobMetaDataEntity> FileUploadJobMetaDataEntities { get; set; }

        public DbSet<JobEmailTemplateEntity> JobEmailTemplates { get; set; }

        public DbSet<JobTypeEntity> JobTypes { get; set; }

        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobEntity>().ToTable("Job");
            modelBuilder.Entity<FileUploadJobMetaDataEntity>().ToTable("FileUploadJobMetaData");
            modelBuilder.Entity<JobEmailTemplateEntity>().ToTable("JobEmailTemplateEntity")
                .HasKey(x => new { x.TemplateId, x.JobStatus });
            modelBuilder.Entity<JobEmailTemplateEntity>().ToTable("JobType");
            modelBuilder.Entity<Schedule>().ToTable("Schedule");
        }
    }
}