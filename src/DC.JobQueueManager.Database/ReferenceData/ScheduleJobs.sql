
	
/*
DECLARE @SummaryOfChanges_Schedule TABLE ([Id] INT, [Action] VARCHAR(100));

MERGE INTO [dbo].[Schedule] AS Target
USING (
*/

			SELECT TOP 100 PERCENT
			       [Id]
				  ,[JobTypeId]
				  ,[JobTitle]
				  ,[Enabled]
				  ,[MinuteIsCadence]
				  ,[Minute]
				  ,[Hour]
				  ,[DayOfTheMonth]
				  ,[Month]
				  ,[DayOfTheWeek]
				  ,[ExecuteOnceOnly]
				  ,NULL AS [LastExecuteDateTime]
			FROM
			(
				  SELECT 'ReferenceData FCS' as [JobTitle], 1 as Id, 1 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute],    9 as [Hour], NULL as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 2 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    1 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 3 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    2 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 4 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    3 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 5 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    4 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 6 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    5 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 7 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    6 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			UNION SELECT 'ReferenceData FCS' as [JobTitle], 8 as Id, 0 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute], NULL as [Hour],    7 as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]

			UNION SELECT 'ReferenceData EPA' as [JobTitle], 10 as Id, 1 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute],    9 as [Hour], NULL as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]

			UNION SELECT 'ReferenceData ULN' as [JobTitle], 20 as Id, 1 as [Enabled], 0 as [MinuteIsCadence], 15 as [Minute],    9 as [Hour], NULL as [DayOfTheMonth], NULL as[Month], NULL as [DayOfTheWeek], 0 as [ExecuteOnceOnly]
			) as Dat
			INNER JOIN [dbo].[JobType] JT 
				ON JT.[Title] = Dat.[JobTitle]

			ORDER BY 1 ASC

/*

	)
	AS Source([Id], [Description], [ConcurrentExecutionCount])
	ON Target.[Id] = Source.[Id]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[JobTypeGroupId]
							  ,Target.[Enabled]				
							  ,Target.[MinuteIsCadence]		
							  ,Target.[Minute]		
							  ,Target.[Hour]		
							  ,Target.[DayOfTheMonth]		
							  ,Target.[Month]		
							  ,Target.[DayOfTheWeek]	
							--  ,Target.[LastExecuteDateTime]					  
					EXCEPT 
						SELECT Source.[JobTypeGroupId]
							  ,Source.[Enabled]
							  ,Source.[MinuteIsCadence]
							  ,Source.[Minute]
							  ,Source.[Hour]
							  ,Source.[DayOfTheMonth]
							  ,Source.[Month]
							  ,Source.[DayOfTheWeek]
							  ,Source.[Enabled]
							--  ,Source.[LastExecuteDateTime]
				)

		  THEN UPDATE SET Target.[Enabled] = Source.[Enabled],
			              Target.[MinuteIsCadence] = Source.[MinuteIsCadence],
			              Target.[Minute] = Source.[Minute],
			              Target.[Hour] = Source.[Hour],
			              Target.[DayOfTheMonth] = Source.[DayOfTheMonth],
			              Target.[Month] = Source.[Month],
			              Target.[DayOfTheWeek] = Source.[DayOfTheWeek],
			              Target.[JobTypeId] = Source.[JobTypeId],
			              Target.[ExecuteOnceOnly] = Source.[ExecuteOnceOnly],
			              --Target.[LastExecuteDateTime] = Source.[LastExecuteDateTime]
	WHEN NOT MATCHED BY TARGET THEN INSERT([Id],[Enabled],[MinuteIsCadence],[Minute],[Hour],[DayOfTheMonth],[Month],[DayOfTheWeek],[JobTypeId],[ExecuteOnceOnly],[LastExecuteDateTime]) 
								   VALUES ([Id],[Enabled],[MinuteIsCadence],[Minute],[Hour],[DayOfTheMonth],[Month],[DayOfTheWeek],[JobTypeId],[ExecuteOnceOnly],[LastExecuteDateTime])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT ISNULL(Deleted.[Id],Inserted.[Id]),$action INTO @SummaryOfChanges_Schedule([Id],[Action])
;

	DECLARE @AddCount_ShedJob INT, @UpdateCount_ShedJob INT, @DeleteCount_ShedJob INT
	SET @AddCount_ShedJob  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Schedule WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_ShedJob = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Schedule WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_ShedJob = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Schedule WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		               %s - Added %i - Update %i - Delete %i',10,1,'JobType', @AddCount_ShedJob, @UpdateCount_ShedJob, @DeleteCount_ShedJob) WITH NOWAIT;

*/