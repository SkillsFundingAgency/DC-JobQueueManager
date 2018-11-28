CREATE TABLE [dbo].[JobTypeGroup]
(
	[JobTypeGroupId]           INT           NOT NULL PRIMARY KEY, 
	[Description]              NVARCHAR(MAX) NOT NULL,
    [ConcurrentExecutionCount] INT           NULL DEFAULT 25,    
)
