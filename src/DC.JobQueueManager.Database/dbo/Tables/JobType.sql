CREATE TABLE [dbo].[JobType](
    [JobTypeId]					INT			  NOT NULL,
    [Title]						NVARCHAR(100) NOT NULL, 
    [Description]				NVARCHAR(250) NOT NULL, 
    [IsCrossLoadingEnabled]		BIT			  NOT NULL DEFAULT 0, 
    [ProcessingOverrideFlag]	BIT			  NULL, 
    [JobTypeGroupId]            INT           NOT NULL DEFAULT 0, 
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobType_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobType_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobType_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobType_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_JobType] PRIMARY KEY ([JobTypeId]), 
    CONSTRAINT [FK_JobType_JobTypeGroupId] FOREIGN KEY ([JobTypeGroupId]) REFERENCES [JobTypeGroup]([JobTypeGroupId])
)