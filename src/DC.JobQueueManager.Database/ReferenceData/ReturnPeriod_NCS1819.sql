DECLARE @CollectionNameNCS1819 NVARCHAR(100) = 'NCS1819';
SET NOCOUNT ON;
--DECLARE @MinsToRemove_NCS1819 INT = 715;
DECLARE @DataTable_NCS1819 TABLE ([CollectionId] INT NOT NULL, [PeriodNumber] INT	NOT NULL, [StartDateTimeUTC] DATETIME NOT NULL, [EndDateTimeUTC] DATETIME NOT NULL, [CalendarMonth] INT NOT NULL, [CalendarYear] INT NOT NULL, PRIMARY KEY ([CollectionId],[PeriodNumber]));

DECLARE @CollectionId_NCS1819 INT = (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionNameNCS1819)
DECLARE @SummaryOfChanges_ReturnPeriod_NCS1819 TABLE ([CollectionId] INT, [PeriodNumber] INT, [Action] VARCHAR(100));

;WITH CTE_RAW_Data([PeriodNumber],[StartDateTimeUTC],[EndDateTimeUTC])
AS

(		  SELECT  1 as [PeriodNumber] , CONVERT(DATETIME, N'2018-04-12T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-05-11T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  2 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-05-12T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-06-12T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  3 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-06-13T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-07-11T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  4 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-07-12T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-08-10T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  5 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-08-11T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-09-12T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  6 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-09-13T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-10-10T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  7 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-10-11T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-11-12T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  8 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-11-13T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2018-12-12T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT  9 AS [PeriodNumber] , CONVERT(DATETIME, N'2018-12-13T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2019-01-11T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT 10 AS [PeriodNumber] , CONVERT(DATETIME, N'2019-01-12T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2019-02-12T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT 11 AS [PeriodNumber] , CONVERT(DATETIME, N'2019-02-13T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2019-03-12T18:16:00.000') AS [EndDateTimeUTC]
	UNION SELECT 12 AS [PeriodNumber] , CONVERT(DATETIME, N'2019-03-13T00:00:00.000') AS [StartDateTimeUTC] , CONVERT(DATETIME, N'2019-04-10T18:16:00.000') AS [EndDateTimeUTC]
)
, CTE_Full([CollectionId],[CollectionName],[PeriodNumber],[StartDateTimeUTC],[EndDateTimeUTC])
AS
(	   
	SELECT 	 
		 @CollectionId_NCS1819 as [CollectionNameId]
		,@CollectionNameNCS1819 as [CollectionName]
		,[PeriodNumber] 
		,[StartDateTimeUTC]
		,[EndDateTimeUTC]	
	FROM CTE_RAW_Data
)

INSERT INTO @DataTable_NCS1819([CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear])
SELECT    @CollectionId_NCS1819 as [CollectionId]
		, NewRecords.[PeriodNumber]
		, NewRecords.[StartDateTimeUTC]
		, NewRecords.[EndDateTimeUTC]
		, MONTH([StartDateTimeUTC]) as [CalendarMonth]
		, YEAR([StartDateTimeUTC]) as [CalendarYear]
FROM CTE_Full NewRecords

--SELECT * FROM @DataTable_NCS1819

BEGIN
	BEGIN TRAN

	BEGIN TRY

		MERGE INTO [dbo].[ReturnPeriod] AS Target
		USING (
				SELECT  [CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear]
				FROM @DataTable_NCS1819
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
			WHEN NOT MATCHED BY Source AND Target.[CollectionId] = @CollectionId_NCS1819 THEN DELETE
			OUTPUT ISNULL(Inserted.[CollectionId],deleted.[CollectionId]),
				   ISNULL(Inserted.[PeriodNumber],deleted.[PeriodNumber]),
				   $action 
			  INTO @SummaryOfChanges_ReturnPeriod_NCS1819([CollectionId],[PeriodNumber],[Action])
			;

			DECLARE @AddCount_RT_NCS1819 INT, @UpdateCount_RT_NCS1819 INT, @DeleteCount_RT_NCS1819 INT
			SET @AddCount_RT_NCS1819  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_NCS1819 WHERE [Action] = 'Insert' GROUP BY Action),0);
			SET @UpdateCount_RT_NCS1819 = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_NCS1819 WHERE [Action] = 'Update' GROUP BY Action),0);
			SET @DeleteCount_RT_NCS1819 = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_NCS1819 WHERE [Action] = 'Delete' GROUP BY Action),0);

			RAISERROR('		      %s : %s - Added %i - Update %i - Delete %i',10,1,'    ReturnPeriod',@CollectionNameNCS1819, @AddCount_RT_NCS1819, @UpdateCount_RT_NCS1819, @DeleteCount_RT_NCS1819) WITH NOWAIT;

			--SELECT t.*, soc.Action FROM @DataTable_NCS1819 t LEFT JOIN @SummaryOfChanges_ReturnPeriod_NCS1819 soc ON t.[CollectionId] = soc.[CollectionId] AND t.[PeriodNumber] = soc.[PeriodNumber]
			
			COMMIT
	END TRY
-- 
-------------------------------------------------------------------------------------- 
-- Handle any problems
-------------------------------------------------------------------------------------- 
-- 
	BEGIN CATCH

		DECLARE   @ErrorMessage_NCS1819		NVARCHAR(4000)
				, @ErrorSeverity_NCS1819	INT 
				, @ErrorState_NCS1819		INT
				, @ErrorNumber_NCS1819		INT
						
		SELECT	  @ErrorNumber_NCS1819		= ERROR_NUMBER()
				, @ErrorMessage_NCS1819		= 'Error in :' + ISNULL(OBJECT_NAME(@@PROCID),'') + ' - Error was :' + ERROR_MESSAGE()
				, @ErrorSeverity_NCS1819	= ERROR_SEVERITY()
				, @ErrorState_NCS1819		= ERROR_STATE();
	
		IF (@@TRANCOUNT>0)
			ROLLBACK;
		
		RAISERROR (  @ErrorMessage_NCS1819		-- Message text.
					, @ErrorSeverity_NCS1819	-- Severity.
					, @ErrorState_NCS1819		-- State.
				  );
				  
	END CATCH
-- 
-------------------------------------------------------------------------------------- 
-- All done
-------------------------------------------------------------------------------------- 
-- 
END
 
-- ROLLBACK
-- DBCC CHECKIDENT ('dbo.ReturnPeriod', RESEED, 1);  
-- SELECT * FROM [dbo].[ReturnPeriod]


