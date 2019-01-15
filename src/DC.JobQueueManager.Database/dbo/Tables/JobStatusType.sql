CREATE TABLE [dbo].[JobStatusType](
    [StatusId]			INT			  NOT NULL,
    [StatusTitle]		NVARCHAR(100) NOT NULL, 
    [StatusDescription] NVARCHAR(250) NOT NULL, 
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_JobStatusType_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_JobStatusType_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_JobStatusType_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_JobStatusType_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_JobStatusType] PRIMARY KEY ([StatusId])
) ON [PRIMARY]