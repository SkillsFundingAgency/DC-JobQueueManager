
CREATE TABLE [dbo].[Job](
	[JobId] [bigint] IDENTITY(1,1) NOT NULL,
	[JobType] SMALLINT NOT NULL,
	[FileName] [varchar](50) NULL,
	[Priority] [smallint] NOT NULL,
	[DateTimeSubmittedUTC] [datetime] NOT NULL,
	[DateTimeUpdatedUTC] [datetime] NULL,
	[Ukprn] [bigint] NULL,
	[StorageReference] [varchar](50) NULL,
	[Status] [smallint] NOT NULL,
	[Rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Job_memoryoptimizedtable] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

