using System.Threading.Tasks;

namespace ESFA.DC.JobNotifications.Interfaces
{
    public interface IEmailNotifier : INotifier
    {
        Task<string> SendEmail(string toEmail, string templateId, System.Collections.Generic.Dictionary<string, dynamic> parameters);
    }
}