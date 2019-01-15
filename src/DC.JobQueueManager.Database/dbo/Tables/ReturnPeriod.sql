CREATE TABLE [dbo].[ReturnPeriod] (
    [ReturnPeriodId]   INT          NOT NULL IDENTITY(1,1),
    [StartDateTimeUTC] DATETIME     NOT NULL,
    [EndDateTimeUTC]   DATETIME     NOT NULL,
    [PeriodNumber]     INT			NOT NULL,
    [CollectionId]     INT          NOT NULL,
    [CalendarMonth]    INT          NOT NULL,
    [CalendarYear]     INT          NOT NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_ReturnPeriod_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_ReturnPeriod_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_ReturnPeriod_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_ReturnPeriod_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_ReturnPeriod] PRIMARY KEY CLUSTERED ([ReturnPeriodId] ASC),
    CONSTRAINT [FK_ReturnPeriod_Collection] FOREIGN KEY ([CollectionId]) REFERENCES [dbo].[Collection] ([CollectionId]),
	CONSTRAINT UC_ReturnPeriod_Key UNIQUE ([CollectionId],[ReturnPeriodId])
);

