
CREATE TABLE [dbo].[JobEmailTemplate](
	[TemplateId] [varchar](500) NOT NULL,
	[JobStatus] [smallint] NOT NULL,
	[Active] [bit] NOT NULL DEFAULT 1,
 [JobType] SMALLINT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_EmailTemplate] PRIMARY KEY CLUSTERED 
(
	[TemplateId] ASC,
	[JobStatus] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO