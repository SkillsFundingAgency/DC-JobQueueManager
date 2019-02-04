CREATE TABLE [dbo].[JobTopicSubscription] (
    [JobTopicId]           INT            NOT NULL,
    [JobTypeId]    SMALLINT       NOT NULL,    
	[CollectionId] int NOT NULL,
	[TopicName]    NVARCHAR (500) NOT NULL,
	[SubscriptionName]    NVARCHAR (500) NOT NULL,
    [TopicOrder]   SMALLINT       CONSTRAINT [DF_JobTopic_TopicOrder] DEFAULT 1 NOT NULL,
    [IsFirstStage] BIT            NULL ,
    [Enabled]      BIT            CONSTRAINT [DF_JobTopic_Enabled] DEFAULT 1 NOT NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobTopic_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobTopic_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobTopic_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobTopic_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_JobTopicSubscription] PRIMARY KEY CLUSTERED ([JobTopicId] ASC),
    CONSTRAINT [IX_JobTopicSubscription] UNIQUE NONCLUSTERED ([JobTopicId] ASC)
);

