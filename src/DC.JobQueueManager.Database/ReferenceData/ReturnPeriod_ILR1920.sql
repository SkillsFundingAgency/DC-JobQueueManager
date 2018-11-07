
/*
DECLARE @CollectionNameILR1920 NVARCHAR(100) = 'ILR1920';
SET NOCOUNT ON;
DECLARE @MinsToRemove_ILR1920 INT = 715;
DECLARE @DataTable_ILR1920 TABLE ([CollectionId] INT NOT NULL, [PeriodNumber] INT	NOT NULL, [StartDateTimeUTC] DATETIME NOT NULL, [EndDateTimeUTC] DATETIME NOT NULL, [CalendarMonth] INT NOT NULL, [CalendarYear] INT NOT NULL, PRIMARY KEY ([CollectionId],[PeriodNumber]));

DECLARE @CollectionId_ILR1920 INT = (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionNameILR1920)
DECLARE @SummaryOfChanges_ReturnPeriod_ILR1920 TABLE ([CollectionId] INT, [PeriodNumber] INT, [Action] VARCHAR(100));

;WITH CTE_RAW_Data([PeriodNumber],[EndDateTimeUTC])
AS

(		  SELECT  1 as [PeriodNumber] , CONVERT(DATETIME, N'2019-09-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  2 as [PeriodNumber] , CONVERT(DATETIME, N'2019-10-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  3 as [PeriodNumber] , CONVERT(DATETIME, N'2019-11-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  4 as [PeriodNumber] , CONVERT(DATETIME, N'2019-12-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  5 as [PeriodNumber] , CONVERT(DATETIME, N'2020-01-07T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  6 as [PeriodNumber] , CONVERT(DATETIME, N'2020-02-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  7 as [PeriodNumber] , CONVERT(DATETIME, N'2020-03-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  8 as [PeriodNumber] , CONVERT(DATETIME, N'2020-04-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  9 as [PeriodNumber] , CONVERT(DATETIME, N'2020-05-07T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 10 as [PeriodNumber] , CONVERT(DATETIME, N'2020-06-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 11 as [PeriodNumber] , CONVERT(DATETIME, N'2020-07-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 12 as [PeriodNumber] , CONVERT(DATETIME, N'2020-08-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 13 as [PeriodNumber] , CONVERT(DATETIME, N'2020-09-13T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 14 as [PeriodNumber] , CONVERT(DATETIME, N'2020-10-17T18:05:00.000') as [EndDateTimeUTC]
)
, CTE_Full([CollectionId],[CollectionName],[PeriodNumber],[StartDateTimeUTC],[EndDateTimeUTC])
AS
(	   
	SELECT 	 
		 @CollectionId_ILR1920 as [CollectionNameId]
		,@CollectionNameILR1920 as [CollectionName]
		,[PeriodNumber] 
		,DATEADD(MI,@MinsToRemove_ILR1920,LAG([EndDateTimeUTC], 1,CONVERT(DATETIME,CONVERT(CHAR(4),YEAR([EndDateTimeUTC])) + '-08-22T18:05:00.000')) OVER (ORDER BY [PeriodNumber])) as [StartDateTimeUTC]
		,[EndDateTimeUTC]	
	FROM CTE_RAW_Data
)

INSERT INTO @DataTable_ILR1920([CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear])
SELECT    @CollectionId_ILR1920 as [CollectionId]
		, NewRecords.[PeriodNumber]
		, NewRecords.[StartDateTimeUTC]
		, NewRecords.[EndDateTimeUTC]
		, MONTH([StartDateTimeUTC]) as [CalendarMonth]
		, YEAR([StartDateTimeUTC]) as [CalendarYear]
FROM CTE_Full NewRecords

--SELECT * FROM @DataTable_ILR1920

BEGIN
	BEGIN TRAN

	BEGIN TRY

		MERGE INTO [dbo].[ReturnPeriod] AS Target
		USING (
				SELECT  [CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear]
				FROM @DataTable_ILR1920
			  )
			AS Source([CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear])
			 ON Target.[CollectionId] = Source.[CollectionId]
			AND Target.[PeriodNumber] = Source.[PeriodNumber]
			WHEN MATCHED 
					AND EXISTS 
						(		SELECT Target.[StartDateTimeUTC]
									  ,Target.[EndDateTimeUTC]
									  ,Target.[CalendarMonth]
									  ,Target.[CalendarYear]
							EXCEPT 
								SELECT Source.[StartDateTimeUTC]
									  ,Source.[EndDateTimeUTC]
									  ,Source.[CalendarMonth]
									  ,Source.[CalendarYear]
						)
				  THEN UPDATE SET Target.[StartDateTimeUTC] = Source.[StartDateTimeUTC],
								  Target.[EndDateTimeUTC] = Source.[EndDateTimeUTC],
								  Target.[CalendarMonth] = Source.[CalendarMonth],
								  Target.[CalendarYear] = Source.[CalendarYear]
			WHEN NOT MATCHED BY TARGET THEN INSERT([StartDateTimeUTC], [EndDateTimeUTC], [PeriodNumber], [CollectionId], [CalendarMonth], [CalendarYear]) 
										   VALUES ([StartDateTimeUTC], [EndDateTimeUTC], [PeriodNumber], [CollectionId], [CalendarMonth], [CalendarYear])
			WHEN NOT MATCHED BY Source AND Target.[CollectionId] = @CollectionId_ILR1920 THEN DELETE
			OUTPUT ISNULL(Inserted.[CollectionId],deleted.[CollectionId]),
				   ISNULL(Inserted.[PeriodNumber],deleted.[PeriodNumber]),
				   $action 
			  INTO @SummaryOfChanges_ReturnPeriod_ILR1920([CollectionId],[PeriodNumber],[Action])
			;

			DECLARE @AddCount_RT_ILR1920 INT, @UpdateCount_RT_ILR1920 INT, @DeleteCount_RT_ILR1920 INT
			SET @AddCount_RT_ILR1920  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_ILR1920 WHERE [Action] = 'Insert' GROUP BY Action),0);
			SET @UpdateCount_RT_ILR1920 = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_ILR1920 WHERE [Action] = 'Update' GROUP BY Action),0);
			SET @DeleteCount_RT_ILR1920 = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_ILR1920 WHERE [Action] = 'Delete' GROUP BY Action),0);

			RAISERROR('		      %s : %s - Added %i - Update %i - Delete %i',10,1,'    ReturnPeriod',CollectionNameILR1920, @AddCount_RT_ILR1920, @UpdateCount_RT_ILR1920, @DeleteCount_RT_ILR1920) WITH NOWAIT;

			--SELECT t.*, soc.Action FROM @DataTable_ILR1920 t LEFT JOIN @SummaryOfChanges_ReturnPeriod_ILR1920 soc ON t.[CollectionId] = soc.[CollectionId] AND t.[PeriodNumber] = soc.[PeriodNumber]
			
			COMMIT
	END TRY
-- 
-------------------------------------------------------------------------------------- 
-- Handle any problems
-------------------------------------------------------------------------------------- 
-- 
	BEGIN CATCH

		DECLARE   @ErrorMessage_ILR1920		NVARCHAR(4000)
				, @ErrorSeverity_ILR1920	INT 
				, @ErrorState_ILR1920		INT
				, @ErrorNumber_ILR1920		INT
						
		SELECT	  @ErrorNumber_ILR1920		= ERROR_NUMBER()
				, @ErrorMessage_ILR1920		= 'Error in :' + ISNULL(OBJECT_NAME(@@PROCID),'') + ' - Error was :' + ERROR_MESSAGE()
				, @ErrorSeverity_ILR1920	= ERROR_SEVERITY()
				, @ErrorState_ILR1920		= ERROR_STATE();
	
		IF (@@TRANCOUNT>0)
			ROLLBACK;
		
		RAISERROR (  @ErrorMessage_ILR1920		-- Message text.
					, @ErrorSeverity_ILR1920	-- Severity.
					, @ErrorState_ILR1920		-- State.
				  );
				  
	END CATCH
-- 
-------------------------------------------------------------------------------------- 
-- All done
-------------------------------------------------------------------------------------- 
-- 
END

*/ 
-- ROLLBACK
-- DBCC CHECKIDENT ('dbo.ReturnPeriod', RESEED, 1);  
-- SELECT * FROM [dbo].[ReturnPeriod]


