using System;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class JobQueueManagerTests
    {
        [Fact]
        public void AddJob_Null()
        {
            //var manager = new JobQueueManager(It.IsAny<IJobQueueManagerSettings>());
            //Assert.Throws<ArgumentNullException>(() => manager.AddJob(null));
        }

        [Fact]
        public void AddJob_Success()
        {
            //var manager = new JobQueueManager(It.IsAny<IJobQueueManagerSettings>());
            //var options = new DbContextOptionsBuilder<JobQueueDataContext>()
            //    .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
            //    .Options;
            //Assert.Throws<ArgumentNullException>(() => manager.AddJob(null));
        }
    }
}
