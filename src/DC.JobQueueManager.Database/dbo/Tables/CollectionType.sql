CREATE TABLE [dbo].[CollectionType] (
    [CollectionTypeId] INT           NOT NULL,
    [Type]             VARCHAR (50)  NOT NULL,
    [Description]      VARCHAR (250) NOT NULL,
	--[CreatedOn]        DATETIME       CONSTRAINT [def_dbo_CollectionType_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]        NVARCHAR (100) CONSTRAINT [def_dbo_CollectionType_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]       DATETIME       CONSTRAINT [def_dbo_CollectionType_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]       NVARCHAR (100) CONSTRAINT [def_dbo_CollectionType_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_CollectionType] PRIMARY KEY CLUSTERED ([CollectionTypeId] ASC)
);

