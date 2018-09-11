using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IBaseJobManager<T>
    {
        long AddJob(T job);

        T GetJobById(long jobId);

        IEnumerable<T> GetAllJobs();

        void PopulatePersonalisation(long jobId, Dictionary<string, dynamic> personalisation);
    }
}
