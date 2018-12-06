CREATE TABLE [dbo].[JobTopicTask] (
    [JobTopicTaskId]         INT            NOT NULL,
    [JobTopicId] INT            NOT NULL,
    [TaskName]   NVARCHAR (500) NOT NULL,
    [TaskOrder]  SMALLINT       CONSTRAINT [DF_JobTopicTask_TaskOrder] DEFAULT 1 NOT NULL,
    [Enabled]    BIT            CONSTRAINT [DF_JobTopicTask_Enabled] DEFAULT 1 NOT NULL,
    CONSTRAINT [PK_JobTopicTask] PRIMARY KEY CLUSTERED ([JobTopicTaskId] ASC),
    CONSTRAINT [FK_JobTopicTask_JobTopic] FOREIGN KEY ([JobTopicId]) REFERENCES [dbo].[JobTopic] ([JobTopicId])
);

