
CREATE TABLE [dbo].[Job](
	[JobId]					BIGINT			NOT NULL IDENTITY(1,1),
	[JobType]				SMALLINT		NOT NULL,
	[Priority]				SMALLINT		NOT NULL,
	[DateTimeSubmittedUTC]	DATETIME		NOT NULL,
	[DateTimeUpdatedUTC]	DATETIME		NULL,
	[SubmittedBy]			VARCHAR(50)		NULL,
	[Status]				SMALLINT		NOT NULL,
	[RowVersion]			TIMESTAMP		NULL,
    [NotifyEmail]			NVARCHAR(500)	NULL, 
    [CrossLoadingStatus]	SMALLINT		NULL ,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_job_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_job_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_job_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_job_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_Job_memoryoptimizedtable] PRIMARY KEY CLUSTERED ( [JobId] ASC )
)