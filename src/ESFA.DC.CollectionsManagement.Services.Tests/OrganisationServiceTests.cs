using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Collection = ESFA.DC.CollectionsManagement.Models.Collection;
using Organisation = ESFA.DC.CollectionsManagement.Models.Organisation;

namespace ESFA.DC.CollectionsManagement.Services.Tests
{
    public class OrganisationServiceTests
    {
        [Fact]
        public void Test_GetAvailableCollectionTypes_NoCollectionFound()
        {
            var dbContextOptions = GetContextOptions();
            var service = new OrganisationService(dbContextOptions);

            SetupData(dbContextOptions);

            var result = service.GetAvailableCollectionTypesAsync(99999).Result;

            result.Should().NotBeNull();
            result.Count().Should().Be(0);
        }

        [Fact]
        public void Test_GetAvailableCollectionTypes_Success()
        {
            var dbContextOptions = GetContextOptions();
            var service = new OrganisationService(dbContextOptions);

            SetupData(dbContextOptions);

            var result = service.GetAvailableCollectionTypesAsync(1000).Result.ToList();

            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Description.Should().Be("ILR collection");
            result[0].Type.Should().Be("ILR");
        }

        [Fact]
        public void Test_GetAvailableCollections_NotFound_CollectionType()
        {
            var dbContextOptions = GetContextOptions();
            var service = new OrganisationService(dbContextOptions);

            SetupData(dbContextOptions);

            var result = service.GetAvailableCollectionsAsync(1000, "EAS011").Result.ToList();

            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }

        [Fact]
        public void Test_GetAvailableCollections_NotFound_Ukprn()
        {
            var dbContextOptions = GetContextOptions();
            var service = new OrganisationService(dbContextOptions);

            SetupData(dbContextOptions);

            var result = service.GetAvailableCollectionsAsync(99999, "ILR").Result.ToList();

            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }

        [Fact]
        public void Test_GetAvailableCollections_Success()
        {
            var dbContextOptions = GetContextOptions();
            var service = new OrganisationService(dbContextOptions);

            SetupData(dbContextOptions);

            var result = service.GetAvailableCollectionsAsync(1000, "ILR").Result.ToList();

            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].CollectionType.Should().Be("ILR");
            result[0].IsOpen.Should().Be(true);
            result[0].CollectionTitle.Should().Be("test coll");
        }

        [Fact]
        public void Test_GetCollection_Success()
        {
            var dbContextOptions = GetContextOptions();
            var service = new OrganisationService(dbContextOptions);

            SetupData(dbContextOptions);

            var result = service.GetCollectionAsync(1000, "test coll").Result;

            result.Should().NotBeNull();
            result.CollectionType.Should().Be("ILR");
            result.IsOpen.Should().Be(true);
            result.CollectionTitle.Should().Be("test coll");
        }

        private void SetupData(DbContextOptions dbContextOptions)
        {
            using (var cmContext = new JobQueueDataContext(dbContextOptions))
            {
                cmContext.Organisations.Add(new OrganisationEntity()
                {
                    Ukprn = 1000,
                    OrgId = "test_org1",
                    OrganisationId = 1
                });

                cmContext.Collections.Add(new CollectionEntity()
                {
                    CollectionId = 1,
                    CollectionTypeId = 1,
                    IsOpen = true,
                    Name = "test coll"
                });

                cmContext.Collections.Add(new CollectionEntity()
                {
                    CollectionId = 2,
                    CollectionTypeId = 1,
                    IsOpen = true,
                    Name = "test coll2"
                });

                cmContext.CollectionTypes.Add(new CollectionTypeEntity()
                {
                    CollectionTypeId = 1,
                    Description = "ILR collection",
                    Type = "ILR"
                });

                cmContext.OrganisationCollections.Add(new OrganisationCollectionEntity()
                {
                    CollectionId = 1,
                    OrganisationId = 1
                });

                cmContext.SaveChanges();
            }
        }

        private DbContextOptions GetContextOptions([CallerMemberName]string functionName = "")
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<JobQueueDataContext>()
                .UseInMemoryDatabase(functionName)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
            return options;
        }
    }
}
