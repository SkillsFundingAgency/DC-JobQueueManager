﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public class EmailTemplateManager : IEmailTemplateManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IReturnCalendarService _returnCalendarService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EmailTemplateManager(
            DbContextOptions contextOptions,
            IReturnCalendarService returnCalendarService,
            IDateTimeProvider dateTimeProvider)
        {
            _contextOptions = contextOptions;
            _returnCalendarService = returnCalendarService;
            _dateTimeProvider = dateTimeProvider;
        }

        public string GetTemplate(long jobId, JobStatusType status, JobType jobType, DateTime dateTimeJobSubmittedUtc)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var job = context.FileUploadJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId);

                //TODO:////
                v1ar period = GetReturnPeriod(job.CollectionName, dateTimeJobSubmittedUtc);
                var emailTemplate =
                    context.JobEmailTemplates.SingleOrDefault(
                        x => x.JobType == (short)jobType && x.JobStatus == (short)status && x.Active);

                if (emailTemplate == null)
                {
                    return string.Empty;
                }

                if (period != null)
                {
                    return emailTemplate.TemplateOpenPeriod;
                }

                return emailTemplate.TemplateClosePeriod ?? emailTemplate.TemplateOpenPeriod;
            }
        }

        public ReturnPeriod GetReturnPeriod(string collectionName, DateTime dateTimeUtc)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                return null;
            }

            return _returnCalendarService.GetPeriodAsync(collectionName, _dateTimeProvider.ConvertUtcToUk(dateTimeUtc)).Result;
        }
    }
}
