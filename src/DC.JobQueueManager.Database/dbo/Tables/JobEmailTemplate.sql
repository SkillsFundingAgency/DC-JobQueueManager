
CREATE TABLE [dbo].[JobEmailTemplate](
	[Id]					INT			 NOT NULL IDENTITY,
	[TemplateOpenPeriod]	VARCHAR(500) NOT NULL,
	 [TemplateClosePeriod]	VARCHAR(500) NULL , 
	[JobStatus]				SMALLINT	 NOT NULL,
	[Active]				BIT			 NOT NULL DEFAULT 1,
	[JobType]				SMALLINT	 NOT NULL DEFAULT 1 
    CONSTRAINT [PK_JobEmailTemplate] PRIMARY KEY ([Id]) 
) ON [PRIMARY]
GO