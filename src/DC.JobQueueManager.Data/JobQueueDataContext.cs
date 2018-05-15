﻿using System;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobEntity>().ToTable("Job");
        }
    }
}