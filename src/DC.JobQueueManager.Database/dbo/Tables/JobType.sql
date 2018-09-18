CREATE TABLE [dbo].[JobType](
    [JobTypeId] [int] NOT NULL,
    [Title] [nvarchar](100) NOT NULL, 
    [Description] [nvarchar](250) NOT NULL, 
    CONSTRAINT [PK_JobType] PRIMARY KEY ([JobTypeId])
) ON [PRIMARY]