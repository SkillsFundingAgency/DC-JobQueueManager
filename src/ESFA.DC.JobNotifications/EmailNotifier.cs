﻿using System;
using System.Collections.Generic;
using ESFA.DC.Job.Models;
using ESFA.DC.JobNotifications.Interfaces;
using Notify.Client;

namespace ESFA.DC.JobNotifications
{
    public class EmailNotifier : IEmailNotifier
    {
        private readonly INotifierConfig _config;

        public EmailNotifier(INotifierConfig config)
        {
            _config = config;
        }

        public string SendEmail(string toEmail, string templateId, Dictionary<string, dynamic> parameters)
        {
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                throw new ArgumentException("Api key is empty");
            }

            var client = new NotificationClient(_config.ApiKey);
            var response = client.SendEmail(toEmail, templateId, parameters);

            return response.reference;
        }

        public string SendEmail(string templateId, Job.Models.Job job, FileUploadJobMetaData metaData)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "JobId", job.JobId },
                { "FileName", metaData.FileName },
                { "CollectionName", metaData.CollectionName },
                { "Name", job.SubmittedBy }
            };
            return SendEmail(job.NotifyEmail, templateId, personalisation);
        }
    }
}
