CREATE TABLE [dbo].[JobStatusType](
    [StatusId] [int] NOT NULL,
    [StatusTitle] [nvarchar](100) NOT NULL, 
    [StatusDescription] [nvarchar](250) NOT NULL, 
    CONSTRAINT [PK_JobStatusType] PRIMARY KEY ([StatusId])
) ON [PRIMARY]