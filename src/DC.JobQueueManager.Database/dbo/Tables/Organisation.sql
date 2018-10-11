CREATE TABLE [dbo].[Organisation] (
    [OrganisationId] INT           NOT NULL,
    [OrgId]          VARCHAR (50)  NOT NULL,
    [Ukprn]          BIGINT        NOT NULL,
    [Name]           VARCHAR (250) NOT NULL,
    [Email]          VARCHAR (250) NULL,
    CONSTRAINT [PK_Organisation] PRIMARY KEY CLUSTERED ([OrganisationId] ASC)
);

