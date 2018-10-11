CREATE TABLE [dbo].[OrganisationCollection] (
    [OrganisationId] INT NOT NULL,
    [CollectionId]   INT NOT NULL,
    CONSTRAINT [PK_OrganisationCollection] PRIMARY KEY CLUSTERED ([OrganisationId] ASC, [CollectionId] ASC),
    CONSTRAINT [FK_OrganisationCollection_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [dbo].[Collection] ([CollectionId]),
    CONSTRAINT [FK_OrganisationCollection_Organisation] FOREIGN KEY ([OrganisationId]) REFERENCES [dbo].[Organisation] ([OrganisationId])
);

