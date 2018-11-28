CREATE TABLE [dbo].[Collection] (
    [CollectionId]     INT          NOT NULL,
    [Name]             VARCHAR (50) NOT NULL,
    [IsOpen]           BIT          NOT NULL,
    [CollectionTypeId] INT          NOT NULL,
    [CollectionYear]   INT          NOT NULL DEFAULT 1819, 
    CONSTRAINT [PK_Collection] PRIMARY KEY CLUSTERED ([CollectionId] ASC),
    CONSTRAINT [FK_Collection_CollectionType] FOREIGN KEY ([CollectionTypeId]) REFERENCES [dbo].[CollectionType] ([CollectionTypeId])
);