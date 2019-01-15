CREATE TABLE [dbo].[Collection] (
    [CollectionId]     INT          NOT NULL,
    [Name]             VARCHAR (50) NOT NULL,
    [IsOpen]           BIT          NOT NULL,
    [CollectionTypeId] INT          NOT NULL,
    [CollectionYear]   INT          NOT NULL DEFAULT 1819, 
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_Collection_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_Collection_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_Collection_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_Collection_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_Collection] PRIMARY KEY CLUSTERED ([CollectionId] ASC),
    CONSTRAINT [FK_Collection_CollectionType] FOREIGN KEY ([CollectionTypeId]) REFERENCES [dbo].[CollectionType] ([CollectionTypeId])
);