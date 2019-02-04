CREATE TABLE [dbo].[JobSubscriptionTask] (
    [JobTopicTaskId]         INT            NOT NULL,
    [JobTopicId] INT            NOT NULL,
    [TaskName]   NVARCHAR (500) NOT NULL,
    [TaskOrder]  SMALLINT       CONSTRAINT [DF_JobTopicTask_TaskOrder] DEFAULT 1 NOT NULL,
    [Enabled]    BIT            CONSTRAINT [DF_JobTopicTask_Enabled] DEFAULT 1 NOT NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobTopicTask_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobTopicTask_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobTopicTask_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobTopicTask_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_JobSubscriptionTask] PRIMARY KEY CLUSTERED ([JobTopicTaskId] ASC),
	CONSTRAINT [IX_JobSubscriptionTask] UNIQUE NONCLUSTERED ([JobTopicTaskId] ASC),
	CONSTRAINT [FK_JobSubscriptionTask_JobTopic] FOREIGN KEY ([JobTopicId]) REFERENCES [dbo].[JobTopicSubscription] ([JobTopicId])
);

