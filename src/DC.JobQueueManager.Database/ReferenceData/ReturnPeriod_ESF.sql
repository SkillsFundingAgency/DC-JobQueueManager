
DECLARE @CollectionNameESF NVARCHAR(100) = 'ESF';
SET NOCOUNT ON;
DECLARE @MinsToRemove_ESF INT = 715;
DECLARE @DataTable_ESF TABLE ([CollectionId] INT NOT NULL, [PeriodNumber] INT	NOT NULL, [StartDateTimeUTC] DATETIME NOT NULL, [EndDateTimeUTC] DATETIME NOT NULL, [CalendarMonth] INT NOT NULL, [CalendarYear] INT NOT NULL, PRIMARY KEY ([CollectionId],[PeriodNumber]));

DECLARE @CollectionId_ESF INT = (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionNameESF)
DECLARE @SummaryOfChanges_ReturnPeriod_ESF TABLE ([CollectionId] INT, [PeriodNumber] INT, [Action] VARCHAR(100));

;WITH CTE_RAW_Data([PeriodNumber],[EndDateTimeUTC])
AS
(
		  SELECT  1 as [PeriodNumber] , CONVERT(DATETIME, N'2018-02-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  2 as [PeriodNumber] , CONVERT(DATETIME, N'2018-03-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  3 as [PeriodNumber] , CONVERT(DATETIME, N'2018-04-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  4 as [PeriodNumber] , CONVERT(DATETIME, N'2018-05-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  5 as [PeriodNumber] , CONVERT(DATETIME, N'2018-06-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  6 as [PeriodNumber] , CONVERT(DATETIME, N'2018-07-05T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  7 as [PeriodNumber] , CONVERT(DATETIME, N'2018-08-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  8 as [PeriodNumber] , CONVERT(DATETIME, N'2018-09-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  9 as [PeriodNumber] , CONVERT(DATETIME, N'2018-10-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 10 as [PeriodNumber] , CONVERT(DATETIME, N'2018-11-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 11 as [PeriodNumber] , CONVERT(DATETIME, N'2018-12-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 12 as [PeriodNumber] , CONVERT(DATETIME, N'2019-01-07T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 13 as [PeriodNumber] , CONVERT(DATETIME, N'2019-02-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 14 as [PeriodNumber] , CONVERT(DATETIME, N'2019-03-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 15 as [PeriodNumber] , CONVERT(DATETIME, N'2019-04-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 16 as [PeriodNumber] , CONVERT(DATETIME, N'2019-05-07T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 17 as [PeriodNumber] , CONVERT(DATETIME, N'2019-06-06T18:05:00.000') as [EndDateTimeUTC]
)
, CTE_Full([CollectionId],[CollectionName],[PeriodNumber],[StartDateTimeUTC],[EndDateTimeUTC])
AS
(	   
	SELECT 	 
		 @CollectionId_ESF as [CollectionNameId]
		,@CollectionNameESF as [CollectionName]
		,[PeriodNumber] 
		,DATEADD(MI,@MinsToRemove_ESF,LAG([EndDateTimeUTC], 1,CONVERT(DATETIME,CONVERT(CHAR(4),YEAR([EndDateTimeUTC])) + '-01-05T18:05:00.000')) OVER (ORDER BY [PeriodNumber])) as [StartDateTimeUTC]
		,[EndDateTimeUTC]	
	FROM CTE_RAW_Data
)

INSERT INTO @DataTable_ESF([CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear])
SELECT    @CollectionId_ESF as [CollectionId]
		, NewRecords.[PeriodNumber]
		, NewRecords.[StartDateTimeUTC]
		, NewRecords.[EndDateTimeUTC]
		, MONTH([StartDateTimeUTC]) as [CalendarMonth]
		, YEAR([StartDateTimeUTC]) as [CalendarYear]
FROM CTE_Full NewRecords

--SELECT * FROM @DataTable_ESF

BEGIN
	BEGIN TRAN

	BEGIN TRY

		MERGE INTO [dbo].[ReturnPeriod] AS Target
		USING (
				SELECT  [CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear]
				FROM @DataTable_ESF
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
			WHEN NOT MATCHED BY Source AND Target.[CollectionId] = @CollectionId_ESF THEN DELETE
			OUTPUT ISNULL(Inserted.[CollectionId],deleted.[CollectionId]),
				   ISNULL(Inserted.[PeriodNumber],deleted.[PeriodNumber]),
				   $action 
			  INTO @SummaryOfChanges_ReturnPeriod_ESF([CollectionId],[PeriodNumber],[Action])
			;

			DECLARE @AddCount_RT_ESF INT, @UpdateCount_RT_ESF INT, @DeleteCount_RT_ESF INT
			SET @AddCount_RT_ESF  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_ESF WHERE [Action] = 'Insert' GROUP BY Action),0);
			SET @UpdateCount_RT_ESF = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_ESF WHERE [Action] = 'Update' GROUP BY Action),0);
			SET @DeleteCount_RT_ESF = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_ESF WHERE [Action] = 'Delete' GROUP BY Action),0);

			RAISERROR('		      %s : %s    - Added %i - Update %i - Delete %i',10,1,'    ReturnPeriod',@CollectionNameESF, @AddCount_RT_ESF, @UpdateCount_RT_ESF, @DeleteCount_RT_ESF) WITH NOWAIT;

			--SELECT t.*, soc.Action FROM @DataTable_ESF t LEFT JOIN @SummaryOfChanges_ReturnPeriod_ESF soc ON t.[CollectionId] = soc.[CollectionId] AND t.[PeriodNumber] = soc.[PeriodNumber]
			
			COMMIT
	END TRY
-- 
-------------------------------------------------------------------------------------- 
-- Handle any problems
-------------------------------------------------------------------------------------- 
-- 
	BEGIN CATCH

		DECLARE   @ErrorMessage_ESF		NVARCHAR(4000)
				, @ErrorSeverity_ESF	INT 
				, @ErrorState_ESF		INT
				, @ErrorNumber_ESF		INT
						
		SELECT	  @ErrorNumber_ESF		= ERROR_NUMBER()
				, @ErrorMessage_ESF		= 'Error in :' + ISNULL(OBJECT_NAME(@@PROCID),'') + ' - Error was :' + ERROR_MESSAGE()
				, @ErrorSeverity_ESF	= ERROR_SEVERITY()
				, @ErrorState_ESF		= ERROR_STATE();
	
		IF (@@TRANCOUNT>0)
			ROLLBACK;
		
		RAISERROR (  @ErrorMessage_ESF		-- Message text.
					, @ErrorSeverity_ESF	-- Severity.
					, @ErrorState_ESF		-- State.
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


