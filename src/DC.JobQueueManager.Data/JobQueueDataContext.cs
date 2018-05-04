using System;
using System.Collections.Generic;
using DC.JobQueueManager.Data.Entities;
using DC.JobQueueManager.Interfaces;
using DC.JobQueueManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DC.JobQueueManager.Data
{
    public sealed class JobQueueDataContext : DbContext
    {
        private readonly string _connectionstring;

        public JobQueueDataContext(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        public DbSet<JobEntity> Jobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    _connectionstring,
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
