using System;
using System.Collections.Generic;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager.Data
{
    public sealed class JobQueueDataContext : DbContext
    {
        private readonly string _connectionString;

        public JobQueueDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<JobEntity> Jobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    _connectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobEntity>().ToTable("Job");
        }
    }
}