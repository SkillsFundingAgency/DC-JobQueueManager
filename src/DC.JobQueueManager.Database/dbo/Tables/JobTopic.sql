CREATE TABLE [dbo].[JobTopic] (
    [JobTopicId]           INT            NOT NULL,
    [JobTypeId]    SMALLINT       NOT NULL,
    [TopicName]    NVARCHAR (500) NOT NULL,
    [TopicOrder]   SMALLINT       CONSTRAINT [DF_JobTopic_TopicOrder] DEFAULT 1 NOT NULL,
    [IsFirstStage] BIT            NOT NULL DEFAULT 1,
    [Enabled]      BIT            CONSTRAINT [DF_JobTopic_Enabled] DEFAULT 1 NOT NULL,
    CONSTRAINT [PK_JobTopic] PRIMARY KEY CLUSTERED ([JobTopicId] ASC),
    CONSTRAINT [FK_JobTopic_JobTopic] FOREIGN KEY ([JobTopicId]) REFERENCES [dbo].[JobTopic] ([JobTopicId]),
    CONSTRAINT [IX_JobTopic] UNIQUE NONCLUSTERED ([JobTopicId] ASC)
);

