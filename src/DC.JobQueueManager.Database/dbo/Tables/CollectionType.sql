CREATE TABLE [dbo].[CollectionType] (
    [CollectionTypeId] INT           NOT NULL,
    [Type]             VARCHAR (50)  NOT NULL,
    [Description]      VARCHAR (250) NOT NULL,
    CONSTRAINT [PK_CollectionType] PRIMARY KEY CLUSTERED ([CollectionTypeId] ASC)
);

