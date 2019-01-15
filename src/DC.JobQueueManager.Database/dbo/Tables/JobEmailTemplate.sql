
CREATE TABLE [dbo].[JobEmailTemplate](
	[Id]					INT			 NOT NULL IDENTITY,
	[TemplateOpenPeriod]	VARCHAR(500) NOT NULL,
	[TemplateClosePeriod]	VARCHAR(500) NULL , 
	[JobStatus]				SMALLINT	 NOT NULL,
	[Active]				BIT			 NOT NULL DEFAULT 1,
	[JobType]				SMALLINT	 NOT NULL DEFAULT 1 
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobEmailTemplate_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobEmailTemplate_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobEmailTemplate_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobEmailTemplate_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_JobEmailTemplate] PRIMARY KEY ([Id]) 
) ON [PRIMARY]
GO