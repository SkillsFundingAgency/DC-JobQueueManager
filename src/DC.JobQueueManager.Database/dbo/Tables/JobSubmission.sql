CREATE TABLE [dbo].[JobSubmission] (
    [Id]          BIGINT   IDENTITY (1, 1) NOT NULL,
    [JobId]       BIGINT   NOT NULL,
    [DateTimeUTC] DATETIME NOT NULL,
    CONSTRAINT [PK_JobSubmission] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_JobSubmission_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Job] ([JobId])
);

