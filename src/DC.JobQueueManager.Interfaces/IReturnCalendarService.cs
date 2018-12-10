using System;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IReturnCalendarService
    {
        Task<ReturnPeriod> GetCurrentPeriodAsync(string collectionName);

        Task<ReturnPeriod> GetNextPeriodAsync(string collectionName);

        Task<ReturnPeriod> GetPeriodAsync(string collectionName, DateTime dateTimeUTC);

        Task<ReturnPeriod> GetPreviousPeriodAsync(string collectionName, DateTime dateTimeUtc);
    }
}
