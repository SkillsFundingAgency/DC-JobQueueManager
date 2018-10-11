CREATE TABLE [dbo].[Collection] (
    [CollectionId]     INT          NOT NULL,
    [Name]             VARCHAR (50) NOT NULL,
    [IsOpen]           BIT          NOT NULL,
    [CollectionTypeId] INT          NOT NULL,
    CONSTRAINT [PK_Collection] PRIMARY KEY CLUSTERED ([CollectionId] ASC),
    CONSTRAINT [FK_Collection_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [dbo].[Collection] ([CollectionId])
);

