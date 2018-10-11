
GO
DECLARE @CollectionName NVARCHAR(100) = 'ILR1819';
SET NOCOUNT ON;
DECLARE @MinsToRemove INT = 715;
DECLARE @DataTable TABLE ([CollectionId] INT NOT NULL, [PeriodNumber] INT	NOT NULL, [StartDateTimeUTC] DATETIME NOT NULL, [EndDateTimeUTC] DATETIME NOT NULL, [CalendarMonth] INT NOT NULL, [CalendarYear] INT NOT NULL );--PRIMARY KEY ([CollectionId],[PeriodNumber]));

DECLARE @CollectionId INT = (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionName)

;WITH CTE_RAW_Data([PeriodNumber],[EndDateTimeUTC])
AS

(		  SELECT  1 as [PeriodNumber] , CONVERT(DATETIME, N'2018-09-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  2 as [PeriodNumber] , CONVERT(DATETIME, N'2018-10-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  3 as [PeriodNumber] , CONVERT(DATETIME, N'2018-11-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  4 as [PeriodNumber] , CONVERT(DATETIME, N'2018-12-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  5 as [PeriodNumber] , CONVERT(DATETIME, N'2019-01-07T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  6 as [PeriodNumber] , CONVERT(DATETIME, N'2019-02-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  7 as [PeriodNumber] , CONVERT(DATETIME, N'2019-03-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  8 as [PeriodNumber] , CONVERT(DATETIME, N'2019-04-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT  9 as [PeriodNumber] , CONVERT(DATETIME, N'2019-05-07T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 10 as [PeriodNumber] , CONVERT(DATETIME, N'2019-06-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 11 as [PeriodNumber] , CONVERT(DATETIME, N'2019-07-04T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 12 as [PeriodNumber] , CONVERT(DATETIME, N'2019-08-06T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 13 as [PeriodNumber] , CONVERT(DATETIME, N'2019-09-13T18:05:00.000') as [EndDateTimeUTC]
	UNION SELECT 14 as [PeriodNumber] , CONVERT(DATETIME, N'2019-10-17T18:05:00.000') as [EndDateTimeUTC]
)
, CTE_Full([CollectionName],[PeriodNumber],[StartDateTimeUTC],[EndDateTimeUTC])
AS
(	   
	SELECT 	 
		 @CollectionName as [CollectionName]
		,[PeriodNumber] 
		,DATEADD(MI,@MinsToRemove,LAG([EndDateTimeUTC], 1,CONVERT(DATETIME,CONVERT(CHAR(4),YEAR([EndDateTimeUTC])) + '-08-22T18:05:00.000')) OVER (ORDER BY [PeriodNumber])) as [StartDateTimeUTC]
		,[EndDateTimeUTC]	
	FROM CTE_RAW_Data
)

INSERT INTO @DataTable([CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear])
SELECT    @CollectionId as [CollectionId]
		, NewRecords.[PeriodNumber]
		, NewRecords.[StartDateTimeUTC]
		, NewRecords.[EndDateTimeUTC]
		, MONTH([StartDateTimeUTC]) as [CalendarMonth]
		, YEAR([StartDateTimeUTC]) as [CalendarYear]
FROM CTE_Full NewRecords

--SELECT * FROM  @DataTable

BEGIN TRAN


BEGIN
	DECLARE @SummaryOfChanges_ReturnPeriod TABLE ([CollectionId] INT, [PeriodNumber] INT, [Action] VARCHAR(100));

	BEGIN TRY

		MERGE INTO [dbo].[ReturnPeriod] AS Target
		USING (
				SELECT  [CollectionId], [PeriodNumber], [StartDateTimeUTC], [EndDateTimeUTC], [CalendarMonth], [CalendarYear]
				FROM @DataTable
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
			WHEN NOT MATCHED BY Source AND Target.[CollectionId] = @CollectionId THEN DELETE
			OUTPUT ISNULL(Inserted.[CollectionId],deleted.[CollectionId]),
				   ISNULL(Inserted.[PeriodNumber],deleted.[PeriodNumber]),
				   $action 
			  INTO @SummaryOfChanges_ReturnPeriod([CollectionId],[PeriodNumber],[Action])
			;

			DECLARE @AddCount_RT INT, @UpdateCount_RT INT, @DeleteCount_RT INT
			SET @AddCount_RT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod WHERE [Action] = 'Insert' GROUP BY Action),0);
			SET @UpdateCount_RT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod WHERE [Action] = 'Update' GROUP BY Action),0);
			SET @DeleteCount_RT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_ReturnPeriod WHERE [Action] = 'Delete' GROUP BY Action),0);

			RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'  ReturnPeriod', @AddCount_RT, @UpdateCount_RT, @DeleteCount_RT) WITH NOWAIT;

			--SELECT t.*, soc.Action FROM @DataTable t LEFT JOIN @SummaryOfChanges_ReturnPeriod soc ON t.[CollectionId] = soc.[CollectionId] AND t.[PeriodNumber] = soc.[PeriodNumber]
			
			COMMIT
	END TRY
-- 
-------------------------------------------------------------------------------------- 
-- Handle any problems
-------------------------------------------------------------------------------------- 
-- 
	BEGIN CATCH

		DECLARE   @ErrorMessage		NVARCHAR(4000)
				, @ErrorSeverity	INT 
				, @ErrorState		INT
				, @ErrorNumber		INT
						
		SELECT	  @ErrorNumber		= ERROR_NUMBER()
				, @ErrorMessage		= 'Error in :' + ISNULL(OBJECT_NAME(@@PROCID),'') + ' - Error was :' + ERROR_MESSAGE()
				, @ErrorSeverity	= ERROR_SEVERITY()
				, @ErrorState		= ERROR_STATE();
	
		IF (@@TRANCOUNT>0)
			ROLLBACK;
		
		RAISERROR (  @ErrorMessage		-- Message text.
					, @ErrorSeverity	-- Severity.
					, @ErrorState		-- State.
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


