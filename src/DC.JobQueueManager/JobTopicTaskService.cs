using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public class JobTopicTaskService : IJobTopicTaskService, IDisposable
    {

        private readonly JobQueueDataContext _context;

        public JobTopicTaskService(DbContextOptions<JobQueueDataContext> dbContextOptions)
        {
            _context = new JobQueueDataContext(dbContextOptions);
        }

        public IEnumerable<ITopicItem> GetTopicItems(JobType jobType, bool isFirstStage = true)
        {
            var topics = new List<TopicItem>();

            var topicsData = _context.JobTopic.Where(x => x.JobTypeId == (short)jobType
                                                      && (!x.IsFirstStage.HasValue || x.IsFirstStage == isFirstStage)
                                                      && x.Enabled == true)
                                                    .OrderBy(x => x.TopicOrder)
                                                    .Include(x => x.JobTopicTask);
            var emptyTaskItem = new TaskItem()
                {
                    Tasks = new List<string>() { string.Empty },
                    SupportsParallelExecution = false
                };

            if (topicsData.Any())
            {
                foreach (var topicEntity in topicsData)
                {
                    var tasks = new List<string>();
                    var topicTaskEntities = topicEntity.JobTopicTask.Where(x => x.Enabled == true)
                        .OrderBy(x => x.TaskOrder).ToList();

                    if (topicTaskEntities.Any())
                    {
                        tasks.AddRange(topicTaskEntities.Select(x => x.TaskName));
                    }

                    var taskItem = new List<ITaskItem>()
                    {
                        tasks.Any() ? new TaskItem(tasks, false) : emptyTaskItem
                    };

                    topics.Add(new TopicItem(topicEntity.TopicName, topicEntity.TopicName, taskItem));
                }
            }

            return topics;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
