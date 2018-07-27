
CREATE TABLE [dbo].[IlrJobMetaData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[FileName] [varchar](50) NULL,
	[FileSize] [decimal](18, 2) NULL,
	[StorageReference] [varchar](100) NULL,
	[IsFirstStage] [bit] NOT NULL,
 [TotalLearners] INT NULL, 
    CONSTRAINT [PK_Job_ilrJobmetadatatable] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY], 
    CONSTRAINT [FK_IlrJobMetaData_ToJob] FOREIGN KEY (JobId) REFERENCES [Job](JobId) 
) ON [PRIMARY]
GO

CREATE INDEX [IX_IlrMetaData_Column] ON [dbo].[IlrJobMetaData] (JobId)
