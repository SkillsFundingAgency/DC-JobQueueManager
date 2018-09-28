
CREATE TABLE [dbo].[FileUploadJobMetaData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[FileName] [varchar](250) NULL,
	[FileSize] [decimal](18, 2) NULL,
	[StorageReference] [varchar](100) NULL,
	[IsFirstStage] [bit] NOT NULL,
    [CollectionName] NVARCHAR(50) NOT NULL DEFAULT 'ILR1819', 
    [PeriodNumber] INT NOT NULL DEFAULT 1, 
    [Ukprn] BIGINT NOT NULL, 
    CONSTRAINT [PK_Job_FileUploadJobMetaData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY], 
    CONSTRAINT [FK_FileUploadJobMetaData_ToJob] FOREIGN KEY (JobId) REFERENCES [Job](JobId) 
) ON [PRIMARY]
GO

CREATE INDEX [IX_FileUploadJobMetaData_Column] ON [dbo].[FileUploadJobMetaData] (JobId)
