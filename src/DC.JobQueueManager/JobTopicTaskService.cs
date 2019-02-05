using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;

namespace ESFA.DC.JobQueueManager
{
    public class JobTopicTaskService : IJobTopicTaskService
    {
        private readonly Func<IJobQueueDataContext> _contextFactory;

        public JobTopicTaskService(Func<IJobQueueDataContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<ITopicItem>> GetTopicItems(JobType jobType, bool isFirstStage = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<JobTopicSubscription> topicsData;

            using (var context = _contextFactory())
            {
                topicsData = await context.JobTopicSubscription.Where(x =>
                        x.JobTypeId == (short) jobType
                        && (!x.IsFirstStage.HasValue || x.IsFirstStage == isFirstStage)
                        && x.Enabled == true)
                    .OrderBy(x => x.TopicOrder)
                    .Include(x => x.JobSubscriptionTask)
                    .ToListAsync(cancellationToken);
            }

            TaskItem emptyTaskItem = new TaskItem()
            {
                Tasks = new List<string>() { string.Empty },
                SupportsParallelExecution = false
            };

            List<TopicItem> topics = new List<TopicItem>();

            if (!topicsData.Any())
            {
                return topics;
            }

            foreach (JobTopicSubscription topicEntity in topicsData)
            {
                List<string> tasks = new List<string>();
                var topicTaskEntities = topicEntity.JobSubscriptionTask.Where(x => x.Enabled == true)
                    .OrderBy(x => x.TaskOrder).ToList();

                if (topicTaskEntities.Any())
                {
                    tasks.AddRange(topicTaskEntities.Select(x => x.TaskName));
                }

                var taskItem = new List<ITaskItem>()
                {
                    tasks.Any() ? new TaskItem(tasks, false) : emptyTaskItem
                };

                topics.Add(new TopicItem(topicEntity.SubscriptionName, topicEntity.TopicName, taskItem));
            }

            return topics;
        }
    }
}
