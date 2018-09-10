using ESFA.DC.Job.Models;

namespace ESFA.DC.JobNotifications.Interfaces
{
    public interface IEmailNotifier : INotifier
    {
        string SendEmail(string toEmail, string templateId, System.Collections.Generic.Dictionary<string, dynamic> parameters);

        string SendEmail(string templateId, Job.Models.Job job, FileUploadJobMetaData metaData);
    }
}