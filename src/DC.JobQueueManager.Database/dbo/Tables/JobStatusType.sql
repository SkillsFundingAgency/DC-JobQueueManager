CREATE TABLE [dbo].[JobStatusType](
    [StatusId]			INT			  NOT NULL,
    [StatusTitle]		NVARCHAR(100) NOT NULL, 
    [StatusDescription] NVARCHAR(250) NOT NULL, 
    CONSTRAINT [PK_JobStatusType] PRIMARY KEY ([StatusId])
) ON [PRIMARY]