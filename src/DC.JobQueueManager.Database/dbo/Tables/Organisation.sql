CREATE TABLE [dbo].[Organisation] (
    [OrganisationId] INT           NOT NULL,
    [OrgId]          VARCHAR (50)  NOT NULL,
    [Ukprn]          BIGINT        NOT NULL,
    [Name]           VARCHAR (250) NOT NULL,
    [Email]          VARCHAR (250) NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_Organisation_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_Organisation_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_Organisation_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_Organisation_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_Organisation] PRIMARY KEY CLUSTERED ([OrganisationId] ASC)
);

