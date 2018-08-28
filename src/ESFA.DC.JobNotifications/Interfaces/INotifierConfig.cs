using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.JobNotifications
{
    public interface INotifierConfig
    {
        string ApiKey { get; }

        string ReplyToEmailAddress { get; }
    }
}
