using System;
using System.Collections.Generic;
using System.Text;

namespace DC.JobQueueManager.Interfaces
{
    public interface IJobQueueManagerSettings
    {
        string ConnectionString { get; set; }
    }
}
