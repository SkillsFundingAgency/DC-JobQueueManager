using System;
using System.Collections.Generic;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ESFA.DC.JobQueueManager.Data
{
    public sealed class JobQueueDataContext : DbContext
    {
        public JobQueueDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<JobEntity> Jobs { get; set; }

        public DbSet<IlrJobMetaDataEntity> IlrJobMetaDataEntities { get; set; }

        public DbSet<JobEmailTemplate> JobEmailTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobEntity>().ToTable("Job");
            modelBuilder.Entity<IlrJobMetaDataEntity>().ToTable("IlrJobMetaData");
            modelBuilder.Entity<JobEmailTemplate>().ToTable("JobEmailTemplate")
                .HasKey(x => new { x.TemplateId, x.JobStatus });
        }
    }
}