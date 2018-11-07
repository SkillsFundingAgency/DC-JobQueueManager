
DECLARE @CollectionNameEAS1819 NVARCHAR(100) = 'EAS1819';
SET NOCOUNT ON;
DECLARE @MinsToRemove_EAS1819 INT = 715;
DECLARE @DataTable_EAS1819 TABLE ([CollectionId] INT NOT NULL, [PeriodNumber] INT	NOT NULL, [StartDateTimeUTC] DATETIME NOT NULL, [EndDateTimeUTC] DATETIME NOT NULL, [CalendarMonth] INT NOT NULL, [CalendarYear] INT NOT NULL, PRIMARY KEY ([CollectionId],[PeriodNumber]));

DECLARE @CollectionId_EAS1819 INT = (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionNameEAS1819)
DECLARE @SummaryOfChanges_ReturnPeriod_EAS1819 TABLE ([CollectionId] INT, [PeriodNumber] INT, [Action] VARCHAR(100));

;WITH CTE_Full([CollectionId],[CollectionName],[PeriodNumber],[StartDateTimeUTC],[EndDateTimeUTC], [CalendarMonth],[CalendarYear] )
AS
(	   
		SELECT  
			 @CollectionId_EAS1819 as [CollectionId]
			,@CollectionNameEAS1819 as [CollectionName]
			,1 as [PeriodNumber]
			,CONVERT(DATETIME, N'2018-08-06T18:05:00.000') AS [StartDateTimeUTC]
			,CONVERT(DATETIME, N'2019-09-06T18:05:00.000') AS [EndDateTimeUTC]	
			,CONVERT(INT, 1) AS [CalendarMonth]	
			,CONVERT(INT, 2018) AS [CalendarYear]	
)

INSERT INTO @DataTable_EAS1819([CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear])
SELECT    @CollectionId_EAS1819 as [CollectionId]
		, NewRecords.[PeriodNumber]
		, NewRecords.[StartDateTimeUTC]
		, NewRecords.[EndDateTimeUTC]
		, NewRecords.[CalendarMonth]
		, NewRecords.[CalendarYear]
FROM CTE_Full NewRecords

--SELECT * FROM @DataTable_EAS1819

BEGIN
	BEGIN TRAN

	BEGIN TRY

		MERGE INTO [dbo].[ReturnPeriod] AS Target
		USING (
				SELECT  [CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear]
				FROM @DataTable_EAS1819
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
			WHEN NOT MATCHED BY Source AND Target.[CollectionId] = @CollectionId_EAS1819 THEN DELETE
			OUTPUT ISNULL(Inserted.[CollectionId],deleted.[CollectionId]),
				   ISNULL(Inserted.[PeriodNumber],deleted.[PeriodNumber]),
				   $action 
			  INTO @SummaryOfChanges_ReturnPeriod_EAS1819([CollectionId],[PeriodNumber],[Action])
			;

			DECLARE @AddCount_RT_EAS1819 INT, @UpdateCount_RT_EAS1819 INT, @DeleteCount_RT_EAS1819 INT
			SET @AddCount_RT_EAS1819  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_EAS1819 WHERE [Action] = 'Insert' GROUP BY Action),0);
			SET @UpdateCount_RT_EAS1819 = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_EAS1819 WHERE [Action] = 'Update' GROUP BY Action),0);
			SET @DeleteCount_RT_EAS1819 = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod_EAS1819 WHERE [Action] = 'Delete' GROUP BY Action),0);

			RAISERROR('		      %s : %s - Added %i - Update %i - Delete %i',10,1,'    ReturnPeriod',@CollectionNameEAS1819, @AddCount_RT_EAS1819, @UpdateCount_RT_EAS1819, @DeleteCount_RT_EAS1819) WITH NOWAIT;

			--SELECT t.*, soc.Action FROM @DataTable_EAS1819 t LEFT JOIN @SummaryOfChanges_ReturnPeriod_EAS1819 soc ON t.[CollectionId] = soc.[CollectionId] AND t.[PeriodNumber] = soc.[PeriodNumber]
			
			COMMIT
	END TRY
-- 
-------------------------------------------------------------------------------------- 
-- Handle any problems
-------------------------------------------------------------------------------------- 
-- 
	BEGIN CATCH

		DECLARE   @ErrorMessage_EAS1819		NVARCHAR(4000)
				, @ErrorSeverity_EAS1819	INT 
				, @ErrorState_EAS1819		INT
				, @ErrorNumber_EAS1819		INT
						
		SELECT	  @ErrorNumber_EAS1819		= ERROR_NUMBER()
				, @ErrorMessage_EAS1819		= 'Error in :' + ISNULL(OBJECT_NAME(@@PROCID),'') + ' - Error was :' + ERROR_MESSAGE()
				, @ErrorSeverity_EAS1819	= ERROR_SEVERITY()
				, @ErrorState_EAS1819		= ERROR_STATE();
	
		IF (@@TRANCOUNT>0)
			ROLLBACK;
		
		RAISERROR (  @ErrorMessage_EAS1819		-- Message text.
					, @ErrorSeverity_EAS1819	-- Severity.
					, @ErrorState_EAS1819		-- State.
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


