
CREATE TABLE [dbo].[Job](
	[JobId] [bigint] IDENTITY(1,1) NOT NULL,
	[JobType] [smallint] NOT NULL,
	[Priority] [smallint] NOT NULL,
	[DateTimeSubmittedUTC] [datetime] NOT NULL,
	[DateTimeUpdatedUTC] [datetime] NULL,
	[SubmittedBy] [varchar](50) NULL,
	[Status] [smallint] NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
    [NotifyEmail] NVARCHAR(500) NULL, 
    [CrossLoadingStatus] SMALLINT NULL , 
    CONSTRAINT [PK_Job_memoryoptimizedtable] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]