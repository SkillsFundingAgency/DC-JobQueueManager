CREATE TABLE [dbo].[JobTypeGroup]
(
	[JobTypeGroupId]           INT           NOT NULL PRIMARY KEY, 
	[Description]              NVARCHAR(MAX) NOT NULL,
    [ConcurrentExecutionCount] INT           NULL DEFAULT 25,    
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobTypeGroup_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobTypeGroup_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobTypeGroup_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobTypeGroup_ModifiedBy] DEFAULT (suser_sname()) NULL,
)
