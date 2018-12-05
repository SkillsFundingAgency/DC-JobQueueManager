using System;
using ESFA.DC.JobQueueManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ESFA.DC.JobQueueManager.Data
{
    public partial class JobQueueDataContext : DbContext
    {
        public JobQueueDataContext()
        {
        }

        public JobQueueDataContext(DbContextOptions<JobQueueDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Collection> Collection { get; set; }
        public virtual DbSet<CollectionType> CollectionType { get; set; }
        public virtual DbSet<FileUploadJobMetaData> FileUploadJobMetaData { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobEmailTemplate> JobEmailTemplate { get; set; }
        public virtual DbSet<JobStatusType> JobStatusType { get; set; }
        public virtual DbSet<JobType> JobType { get; set; }
        public virtual DbSet<JobTypeGroup> JobTypeGroup { get; set; }
        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationCollection> OrganisationCollection { get; set; }
        public virtual DbSet<ReturnPeriod> ReturnPeriod { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }
        public virtual DbSet<JobTopic> JobTopic { get; set; }
        public virtual DbSet<JobTopicTask> JobTopicTask { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Collection>(entity =>
            {
                entity.Property(e => e.CollectionId).ValueGeneratedNever();

                entity.Property(e => e.CollectionYear).HasDefaultValueSql("((1819))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.CollectionType)
                    .WithMany(p => p.Collection)
                    .HasForeignKey(d => d.CollectionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Collection_CollectionType");
            });

            modelBuilder.Entity<CollectionType>(entity =>
            {
                entity.Property(e => e.CollectionTypeId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FileUploadJobMetaData>(entity =>
            {
                entity.HasIndex(e => e.JobId)
                    .HasName("IX_FileUploadJobMetaData_Column");

                entity.Property(e => e.CollectionName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('ILR1819')");

                entity.Property(e => e.CollectionYear).HasDefaultValueSql("((1819))");

                entity.Property(e => e.FileName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.PeriodNumber).HasDefaultValueSql("((1))");

                entity.Property(e => e.StorageReference)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.FileUploadJobMetaData)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FileUploadJobMetaData_ToJob");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(e => e.DateTimeSubmittedUtc)
                    .HasColumnName("DateTimeSubmittedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTimeUpdatedUtc)
                    .HasColumnName("DateTimeUpdatedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.NotifyEmail).HasMaxLength(500);

                entity.Property(e => e.RowVersion)
                    .IsRowVersion();

                entity.Property(e => e.SubmittedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<JobEmailTemplate>(entity =>
            {
                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.JobType).HasDefaultValueSql("((1))");

                entity.Property(e => e.TemplateClosePeriod)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TemplateOpenPeriod)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<JobStatusType>(entity =>
            {
                entity.HasKey(e => e.StatusId);

                entity.Property(e => e.StatusId).ValueGeneratedNever();

                entity.Property(e => e.StatusDescription)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.StatusTitle)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<JobType>(entity =>
            {
                entity.Property(e => e.JobTypeId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.JobTypeGroup)
                    .WithMany(p => p.JobType)
                    .HasForeignKey(d => d.JobTypeGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_JobType_JobTypeGroupId");
            });

            modelBuilder.Entity<JobTypeGroup>(entity =>
            {
                entity.Property(e => e.JobTypeGroupId).ValueGeneratedNever();

                entity.Property(e => e.ConcurrentExecutionCount).HasDefaultValueSql("((25))");

                entity.Property(e => e.Description).IsRequired();
            });

            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.Property(e => e.OrganisationId).ValueGeneratedNever();

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

            modelBuilder.Entity<OrganisationCollection>(entity =>
            {
                entity.HasKey(e => new { e.OrganisationId, e.CollectionId });

                entity.HasOne(d => d.Collection)
                    .WithMany(p => p.OrganisationCollection)
                    .HasForeignKey(d => d.CollectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrganisationCollection_Collection");

                entity.HasOne(d => d.Organisation)
                    .WithMany(p => p.OrganisationCollection)
                    .HasForeignKey(d => d.OrganisationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrganisationCollection_Organisation");
            });

            modelBuilder.Entity<ReturnPeriod>(entity =>
            {
                entity.HasIndex(e => new { e.CollectionId, e.ReturnPeriodId })
                    .HasName("UC_ReturnPeriod_Key")
                    .IsUnique();

                entity.Property(e => e.EndDateTimeUtc)
                    .HasColumnName("EndDateTimeUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.StartDateTimeUtc)
                    .HasColumnName("StartDateTimeUTC")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Collection)
                    .WithMany(p => p.ReturnPeriod)
                    .HasForeignKey(d => d.CollectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReturnPeriod_Collection");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.LastExecuteDateTime).HasColumnType("datetime");

                entity.HasOne(d => d.JobType)
                    .WithMany(p => p.Schedule)
                    .HasForeignKey(d => d.JobTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Schedule_JobType");
            });

            modelBuilder.Entity<JobTopic>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_JobTopic")
                    .IsUnique();

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TopicName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TopicOrder).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<JobTopicTask>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TaskOrder).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.JobTopic)
                    .WithMany(p => p.JobTopicTask)
                    .HasForeignKey(d => d.JobTopicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_JobTopicTask_JobTopic");
            });
        }
    }
}
