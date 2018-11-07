
CREATE TABLE [dbo].[Job](
	[JobId]					BIGINT			NOT NULL IDENTITY(1,1),
	[JobType]				SMALLINT		NOT NULL,
	[Priority]				SMALLINT		NOT NULL,
	[DateTimeSubmittedUTC]	DATETIME		NOT NULL,
	[DateTimeUpdatedUTC]	DATETIME		NULL,
	[SubmittedBy]			VARCHAR(50)		NULL,
	[Status]				SMALLINT		NOT NULL,
	[RowVersion]			TIMESTAMP		NOT NULL,
    [NotifyEmail]			NVARCHAR(500)	NULL, 
    [CrossLoadingStatus]	SMALLINT		NULL , 
    CONSTRAINT [PK_Job_memoryoptimizedtable] PRIMARY KEY CLUSTERED ( [JobId] ASC )
)