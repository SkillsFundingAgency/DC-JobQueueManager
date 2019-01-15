CREATE TABLE [dbo].[JobTopic] (
    [JobTopicId]           INT            NOT NULL,
    [JobTypeId]    SMALLINT       NOT NULL,
    [TopicName]    NVARCHAR (500) NOT NULL,
    [TopicOrder]   SMALLINT       CONSTRAINT [DF_JobTopic_TopicOrder] DEFAULT 1 NOT NULL,
    [IsFirstStage] BIT            NULL ,
    [Enabled]      BIT            CONSTRAINT [DF_JobTopic_Enabled] DEFAULT 1 NOT NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobTopic_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobTopic_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobTopic_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobTopic_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_JobTopic] PRIMARY KEY CLUSTERED ([JobTopicId] ASC),
    CONSTRAINT [FK_JobTopic_JobTopic] FOREIGN KEY ([JobTopicId]) REFERENCES [dbo].[JobTopic] ([JobTopicId]),
    CONSTRAINT [IX_JobTopic] UNIQUE NONCLUSTERED ([JobTopicId] ASC)
);

