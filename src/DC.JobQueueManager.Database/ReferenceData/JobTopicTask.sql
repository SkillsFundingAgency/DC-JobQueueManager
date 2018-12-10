
BEGIN

	DECLARE @SummaryOfChanges_JobTopicTasks TABLE ([JobTopicTaskId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[JobTopicTask] AS Target
	USING (VALUES
			( 1, 3,  N'TaskGenerateValidationReport', 1, 1),
			( 2, 3,  N'TaskGenerateDataMatchReport', 1, 0),
			( 3, 6,  N'ALB', 1, 1),
			( 4, 6,  N'FM25', 2, 1),
			( 5, 6,  N'FM35', 3, 1),
			( 6, 6,  N'FM36', 4, 1),
			( 7, 6,  N'FM70', 5, 1),
			( 8, 6,  N'FM81', 7, 1),
			( 9, 7,  N'PersistDataToDeds', 1, 1),
			(10, 8,  N'TaskGenerateDataMatchReport', 1, 0),
			(11, 8,  N'TaskGenerateValidationReport', 2, 1),
			(12, 8,  N'TaskGenerateAllbOccupancyReport', 3, 1),
			(13, 8,  N'TaskGenerateFundingSummaryReport', 4, 1),
			(14, 8,  N'TaskGenerateMainOccupancyReport', 5, 1),
			(15, 8,  N'TaskGenerateMathsAndEnglishReport', 6, 1),
			(16, 8,  N'TaskGenerateAppsAdditionalPaymentsReport', 7, 1),
			(17, 8,  N'TaskGenerateAppsIndicativeEarningsReport', 8, 1),
			(18, 8,  N'TaskGenerateTrailblazerEmployerIncentivesReport', 9, 1),
			(19, 9,  N'Validation', 1, 1),
			(20, 9,  N'Storage', 2, 1),
			(21, 9,  N'Reporting', 3, 1),
			(22, 10, N'TaskGenerateAdultFundingClaimReport', 1, 1),
			(23, 10, N'TaskGenerateFundingSummaryReport', 2, 1),
			(24, 11, N'Validation', 1, 1),
			(25, 11, N'Storage', 2, 1),
			(26, 11, N'Reporting', 3, 1),
			(27, 8,  N'TaskGenerateFundingClaim1619Report', 10, 1)
		  )
		AS Source([JobTopicTaskId],[JobTopicId],[TaskName],[TaskOrder],[Enabled])
		ON Target.[JobTopicTaskId] = Source.[JobTopicTaskId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[JobTopicTaskId] 
								  ,Target.[JobTopicId]
								  ,Target.[TaskName]
								  ,Target.[TaskOrder]
								  ,Target.[Enabled]
						EXCEPT 
							SELECT Source.[JobTopicTaskId]
								  ,Source.[JobTopicId]
								  ,Source.[TaskName]
								  ,Source.[TaskOrder]
								  ,Source.[Enabled]
					)
			  THEN UPDATE SET Target.[JobTopicId] = Source.[JobTopicId],
							  Target.[TaskName] = Source.[TaskName],
							  Target.[TaskOrder] = Source.[TaskOrder],
							  Target.[Enabled] = Source.[Enabled]

		WHEN NOT MATCHED BY TARGET THEN INSERT([JobTopicTaskId],[JobTopicId],[TaskName],[TaskOrder],[Enabled]) 
									   VALUES ([JobTopicTaskId],[JobTopicId],[TaskName],[TaskOrder],[Enabled])
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[JobTopicTaskId],$action INTO @SummaryOfChanges_JobTopicTasks([JobTopicTaskId],[Action])
	;

		DECLARE @AddCount_JTT INT, @UpdateCount_JTT INT, @DeleteCount_JTT INT
		SET @AddCount_JTT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopicTasks WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_JTT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopicTasks WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_JTT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopicTasks WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		        %s - Added %i - Update %i - Delete %i',10,1,'JobTopic', @AddCount_JTT, @UpdateCount_JTT, @DeleteCount_JTT) WITH NOWAIT;
END
