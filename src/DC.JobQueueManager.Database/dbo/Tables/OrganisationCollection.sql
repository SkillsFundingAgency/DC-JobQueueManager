CREATE TABLE [dbo].[OrganisationCollection] (
    [OrganisationId] INT NOT NULL,
    [CollectionId]   INT NOT NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_OrganisationCollection_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_OrganisationCollection_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_OrganisationCollection_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_OrganisationCollection_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_OrganisationCollection] PRIMARY KEY CLUSTERED ([OrganisationId] ASC, [CollectionId] ASC),
    CONSTRAINT [FK_OrganisationCollection_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [dbo].[Collection] ([CollectionId]),
    CONSTRAINT [FK_OrganisationCollection_Organisation] FOREIGN KEY ([OrganisationId]) REFERENCES [dbo].[Organisation] ([OrganisationId])
);

