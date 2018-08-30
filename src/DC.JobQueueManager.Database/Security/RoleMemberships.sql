ALTER ROLE [db_datawriter] ADD MEMBER [JobQueueManagerApiUser];

GO
ALTER ROLE [db_datareader] ADD MEMBER [JobQueueManagerApiUser];

GO
ALTER ROLE [db_datawriter] ADD MEMBER [JobQueueManagerSchedulerUser];

GO
ALTER ROLE [db_datareader] ADD MEMBER [JobQueueManagerSchedulerUser];


