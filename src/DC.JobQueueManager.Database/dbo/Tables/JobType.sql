CREATE TABLE [dbo].[JobType](
    [JobTypeId] [int] NOT NULL,
    [Title] [nvarchar](100) NOT NULL, 
    [Description] [nvarchar](250) NOT NULL, 
    [IsCrossLoadingEnabled] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_JobType] PRIMARY KEY ([JobTypeId])
) ON [PRIMARY]