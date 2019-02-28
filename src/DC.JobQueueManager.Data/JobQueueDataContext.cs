using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.JobQueueManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ESFA.DC.JobQueueManager.Data
{
    public partial class JobQueueDataContext : DbContext, IJobQueueDataContext
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
        public virtual DbSet<Eas> Eas { get; set; }
        public virtual DbSet<Efs> Efs { get; set; }
        public virtual DbSet<FileUploadJobMetaData> FileUploadJobMetaData { get; set; }
        public virtual DbSet<Ilr1819> Ilr1819 { get; set; }
        public virtual DbSet<Ilr1920> Ilr1920 { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobEmailTemplate> JobEmailTemplate { get; set; }
        public virtual DbSet<JobStatusType> JobStatusType { get; set; }
        public virtual DbSet<JobSubmission> JobSubmission { get; set; }
        public virtual DbSet<JobSubscriptionTask> JobSubscriptionTask { get; set; }
        public virtual DbSet<JobTopic> JobTopic { get; set; }
        public virtual DbSet<JobTopicSubscription> JobTopicSubscription { get; set; }
        public virtual DbSet<JobType> JobType { get; set; }
        public virtual DbSet<JobTypeGroup> JobTypeGroup { get; set; }
        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationCollection> OrganisationCollection { get; set; }
        public virtual DbSet<ReturnPeriod> ReturnPeriod { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLOCALDB;Database=JobManagement;Trusted_Connection=True;");
            }
        }


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

            modelBuilder.Entity<Eas>(entity =>
            {
                entity.HasKey(e => e.Ukprn);

                entity.ToTable("EAS", "DataLoad");

                entity.Property(e => e.Ukprn).ValueGeneratedNever();
            });

            modelBuilder.Entity<Efs>(entity =>
            {
                entity.HasKey(e => e.Ukprn);

                entity.ToTable("EFS", "DataLoad");

                entity.Property(e => e.Ukprn).ValueGeneratedNever();
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

            modelBuilder.Entity<Ilr1819>(entity =>
            {
                entity.HasKey(e => e.Ukprn);

                entity.ToTable("ILR1819", "DataLoad");

                entity.Property(e => e.Ukprn).ValueGeneratedNever();
            });

            modelBuilder.Entity<Ilr1920>(entity =>
            {
                entity.HasKey(e => e.Ukprn);

                entity.ToTable("ILR1920", "DataLoad");

                entity.Property(e => e.Ukprn).ValueGeneratedNever();
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

                entity.Property(e => e.RowVersion).IsRowVersion();

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

            modelBuilder.Entity<JobSubmission>(entity =>
            {
                entity.Property(e => e.DateTimeUtc)
                    .HasColumnName("DateTimeUTC")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.JobSubmission)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_JobSubmission_Job");
            });

            modelBuilder.Entity<JobSubscriptionTask>(entity =>
            {
                entity.HasKey(e => e.JobTopicTaskId);

                entity.HasIndex(e => e.JobTopicTaskId)
                    .HasName("IX_JobSubscriptionTask")
                    .IsUnique();

                entity.Property(e => e.JobTopicTaskId).ValueGeneratedNever();

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TaskOrder).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.JobTopic)
                    .WithMany(p => p.JobSubscriptionTask)
                    .HasForeignKey(d => d.JobTopicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_JobSubscriptionTask_JobTopic");
            });

            modelBuilder.Entity<JobTopic>(entity =>
            {
                entity.HasIndex(e => e.JobTopicId)
                    .HasName("IX_JobTopic")
                    .IsUnique();

                entity.Property(e => e.JobTopicId).ValueGeneratedNever();

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TopicName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TopicOrder).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.JobTopicNavigation)
                    .WithOne(p => p.InverseJobTopicNavigation)
                    .HasForeignKey<JobTopic>(d => d.JobTopicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_JobTopic_JobTopic");
            });

            modelBuilder.Entity<JobTopicSubscription>(entity =>
            {
                entity.HasKey(e => e.JobTopicId);

                entity.HasIndex(e => e.JobTopicId)
                    .HasName("IX_JobTopicSubscription")
                    .IsUnique();

                entity.Property(e => e.JobTopicId).ValueGeneratedNever();

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.SubscriptionName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TopicName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TopicOrder).HasDefaultValueSql("((1))");
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

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

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
        }

        public async Task<IList<T>> FromSqlAsync<T>(CommandType commandType, string sql, object parameters)
        {
            using (var connection = new SqlConnection(Database.GetDbConnection().ConnectionString))
            { 
                await connection.OpenAsync();
                
                return (await connection.QueryAsync<T>(sql, parameters,commandType:commandType)).ToList();
            }
        }
    }
}
