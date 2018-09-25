CREATE TABLE [dbo].[Schedule](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Enabled] [bit] NOT NULL,
	[MinuteIsCadence] [bit] NULL,
	[Minute] [tinyint] NULL,
	[Hour] [tinyint] NULL,
	[DayOfTheMonth] [tinyint] NULL,
	[Month] [tinyint] NULL,
	[DayOfTheWeek] [tinyint] NULL,
	[ExternalDataType] [tinyint] NOT NULL,
	[ExecuteOnceOnly] [bit] NOT NULL,
	[LastExecuteDateTime] [datetime] NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
