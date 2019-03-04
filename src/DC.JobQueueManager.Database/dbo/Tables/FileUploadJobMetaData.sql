
CREATE TABLE [dbo].[FileUploadJobMetaData](
	[Id]				BIGINT			 NOT NULL IDENTITY(1,1),
	[JobId]				BIGINT			 NOT NULL,
	[FileName]			VARCHAR(250)	 NULL,
	[FileSize]			DECIMAL(18, 2)	 NULL,
	[StorageReference]	VARCHAR(100)	 NULL,
	[IsFirstStage]		BIT				 NOT NULL,
    [CollectionName]	NVARCHAR(50)	 NOT NULL DEFAULT 'ILR1819', 
    [PeriodNumber]		INT				 NOT NULL DEFAULT 1, 
    [Ukprn]				BIGINT			 NOT NULL, 
    [TermsAccepted]		BIT				 NULL, 
    [CollectionYear]    INT NOT NULL DEFAULT 1819, 
	--[CreatedOn]        DATETIME       CONSTRAINT [def_dbo_FileUploadJobMetaData_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]        NVARCHAR (100) CONSTRAINT [def_dbo_FileUploadJobMetaData_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]       DATETIME       CONSTRAINT [def_dbo_FileUploadJobMetaData_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]       NVARCHAR (100) CONSTRAINT [def_dbo_FileUploadJobMetaData_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_Job_FileUploadJobMetaData] PRIMARY KEY CLUSTERED ([Id] ASC ), 
    CONSTRAINT [FK_FileUploadJobMetaData_ToJob] FOREIGN KEY (JobId) REFERENCES [Job](JobId) 
)
GO

CREATE INDEX [IX_FileUploadJobMetaData_Column] ON [dbo].[FileUploadJobMetaData] (JobId)
