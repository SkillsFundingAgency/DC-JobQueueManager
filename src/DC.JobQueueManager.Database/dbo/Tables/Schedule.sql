CREATE TABLE [dbo].[Schedule] (
    [ID]                  BIGINT         NOT NULL IDENTITY (1, 1),
    [Enabled]             BIT            NOT NULL,
    [MinuteIsCadence]     BIT            NULL,
    [Minute]              TINYINT        NULL,
    [Hour]                TINYINT        NULL,
    [DayOfTheMonth]       TINYINT        NULL,
    [Month]               TINYINT        NULL,
    [DayOfTheWeek]        TINYINT        NULL,
    [JobTypeId]           INT            NOT NULL,
    [ExecuteOnceOnly]     BIT            NOT NULL,
    [LastExecuteDateTime] DATETIME       NULL,
	--[CreatedOn]            DATETIME       CONSTRAINT [def_dbo_Schedule_CreatedOn] DEFAULT (getdate()) NULL,
    --[CreatedBy]            NVARCHAR (100) CONSTRAINT [def_dbo_Schedule_Createdby] DEFAULT (suser_sname()) NULL,
    --[ModifiedOn]           DATETIME       CONSTRAINT [def_dbo_Schedule_ModifiedOn] DEFAULT (getdate()) NULL,
    --[ModifiedBy]           NVARCHAR (100) CONSTRAINT [def_dbo_Schedule_ModifiedBy] DEFAULT (suser_sname()) NULL,
    CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED ([ID] ASC), 
    CONSTRAINT [FK_Schedule_JobType] FOREIGN KEY ([JobTypeId]) REFERENCES [JobType]([JobTypeId])
);

