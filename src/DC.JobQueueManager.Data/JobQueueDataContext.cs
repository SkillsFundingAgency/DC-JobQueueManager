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

        public DbSet<CollectionEntity> Collection { get; set; }

        public DbSet<CollectionEntity> Collections { get; set; }

        public DbSet<CollectionTypeEntity> CollectionTypes { get; set; }

        public DbSet<OrganisationEntity> Organisations { get; set; }

        public DbSet<OrganisationCollectionEntity> OrganisationCollections { get; set; }

        public DbSet<ReturnPeriodEntity> ReturnPeriods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobEntity>().ToTable("Job");
            modelBuilder.Entity<FileUploadJobMetaDataEntity>().ToTable("FileUploadJobMetaData");
            modelBuilder.Entity<JobEmailTemplateEntity>().ToTable("JobEmailTemplate");
            modelBuilder.Entity<JobTypeEntity>().ToTable("JobType");
            modelBuilder.Entity<Schedule>().ToTable("Schedule");

            modelBuilder.Entity<CollectionEntity>(entity =>
            {
                entity.Property(e => e.CollectionId).ValueGeneratedNever();
                entity.ToTable("Collection");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CollectionTypeEntity>(entity =>
            {
                entity.Property(e => e.CollectionTypeId).ValueGeneratedNever();
                entity.ToTable("CollectionType");
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrganisationEntity>(entity =>
            {
                entity.Property(e => e.OrganisationId).ValueGeneratedNever();
                entity.ToTable("Organisation");
                entity.Property(e => e.Email)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.OrgId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrganisationCollectionEntity>(entity =>
            {
                entity.HasKey(e => new { e.OrganisationId, e.CollectionId });
                entity.ToTable("OrganisationCollection");
                entity.HasOne(d => d.CollectionEntity)
                    .WithMany(p => p.OrganisationCollection)
                    .HasForeignKey(d => d.CollectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrganisationCollection_Collection");

                entity.HasOne(d => d.OrganisationEntity)
                    .WithMany(p => p.OrganisationCollection)
                    .HasForeignKey(d => d.OrganisationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrganisationCollection_Organisation");
            });

            modelBuilder.Entity<ReturnPeriodEntity>(entity =>
            {
                entity.Property(e => e.ReturnPeriodId).ValueGeneratedNever();
                entity.ToTable("ReturnPeriod");

                entity.Property(e => e.EndDateTimeUtc)
                    .HasColumnName("EndDateTimeUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.PeriodNumber)
                    .IsRequired();

                entity.Property(e => e.StartDateTimeUtc)
                    .HasColumnName("StartDateTimeUTC")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.CollectionEntity)
                    .WithMany(p => p.ReturnPeriod)
                    .HasForeignKey(d => d.CollectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReturnPeriod_Collection");
            });
        }
    }
}