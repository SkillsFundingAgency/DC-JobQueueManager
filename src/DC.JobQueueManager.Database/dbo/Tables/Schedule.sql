CREATE TABLE [dbo].[Schedule] (
    [ID]                  BIGINT         NOT NULL IDENTITY (1, 1),
    [Enabled]             BIT            NOT NULL,
    [MinuteIsCadence]     BIT            NULL,
    [Minute]              TINYINT        NULL,
    [Hour]                TINYINT        NULL,
    [DayOfTheMonth]       TINYINT        NULL,
    [Month]               TINYINT        NULL,
    [DayOfTheWeek]        TINYINT        NULL,
    [ExternalDataType]    NVARCHAR (MAX) NOT NULL,
    [ExecuteOnceOnly]     BIT            NOT NULL,
    [LastExecuteDateTime] DATETIME       NULL,
    CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED ([ID] ASC)
);

