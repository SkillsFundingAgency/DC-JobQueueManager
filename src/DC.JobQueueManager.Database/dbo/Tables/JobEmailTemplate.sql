
CREATE TABLE [dbo].[JobEmailTemplate](
	[Id] [int] NOT NULL,
	[TemplateOpenPeriod] [varchar](500) NOT NULL,
	 [TemplateClosePeriod] VARCHAR(500) NULL , 
	[JobStatus] [smallint] NOT NULL,
	[Active] [bit] NOT NULL DEFAULT 1,
	[JobType] SMALLINT NOT NULL DEFAULT 1 
    CONSTRAINT [PK_JobEmailTemplate] PRIMARY KEY ([Id]) 
) ON [PRIMARY]
GO