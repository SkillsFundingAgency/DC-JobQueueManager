CREATE TABLE [dbo].[JobType](
    [JobTypeId]					INT			  NOT NULL,
    [Title]						NVARCHAR(100) NOT NULL, 
    [Description]				NVARCHAR(250) NOT NULL, 
    [IsCrossLoadingEnabled]		BIT			  NOT NULL DEFAULT 0, 
    [ProcessingOverrideFlag]	BIT			  NULL, 
    CONSTRAINT [PK_JobType] PRIMARY KEY ([JobTypeId])
)