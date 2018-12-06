
BEGIN

	DECLARE @SummaryOfChanges_JobTopic TABLE ([JobTopicId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[JobTopic] AS Target
	USING (VALUES
			 (1, 1, N'FileValidation', 1, 1, 1),
			(2, 1, N'Validation', 2, 1, 1),
			(3, 1, N'Reports', 3, 1, 1),
			(4, 1, N'FileValidation', 1, 0, 1),
			(5, 1, N'Validation', 2, 0, 1),
			(6, 1, N'Funding', 3, 0, 1),
			(7, 1, N'Deds', 4, 0, 1),
			(8, 1, N'Reports', 5, 0, 1),
			(9, 2, N'Process', 1, 0, 1),
			(10, 2, N'Reports', 1, 0, 1),
			(11, 3, N'Process', 1, 0, 1)
		  )
		AS Source([JobTopicId],[JobTypeId],[TopicName],[TopicOrder],[IsFirstStage],[Enabled] )
		ON Target.[JobTopicId] = Source.[JobTopicId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[JobTopicId]
								  ,Target.[JobTypeId]
								  ,Target.[TopicName]
								  ,Target.[TopicOrder]
								  ,Target.[IsFirstStage]
								  ,Target.[Enabled]
						EXCEPT 
							SELECT Source.[JobTopicId]
								  ,Source.[JobTypeId]
								  ,Source.[TopicName]
								  ,Source.[TopicOrder]
								  ,Source.[IsFirstStage]
								  ,Source.[Enabled]
					)
			  THEN UPDATE SET Target.[JobTypeId] = Source.[JobTypeId],
							  Target.[TopicName] = Source.[TopicName],
							  Target.[TopicOrder] = Source.[TopicOrder],
							  Target.[IsFirstStage] = Source.[IsFirstStage],
							  Target.[Enabled] = Source.[Enabled]

		WHEN NOT MATCHED BY TARGET THEN INSERT([JobTopicId],[JobTypeId],[TopicName],[TopicOrder],[IsFirstStage],[Enabled] ) 
									   VALUES ([JobTopicId],[JobTypeId],[TopicName],[TopicOrder],[IsFirstStage],[Enabled] )
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[JobTopicId],$action INTO @SummaryOfChanges_JobTopic([JobTopicId],[Action])
	;

		DECLARE @AddCount_JBT INT, @UpdateCount_JBT INT, @DeleteCount_JBT INT
		SET @AddCount_JBT  = ISNULL((SELECT COUNT(*) FROM @SummaryOfChanges_JobTopic WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_JBT = ISNULL((SELECT COUNT(*) FROM @SummaryOfChanges_JobTopic WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_JBT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopic WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		        %s - Added %i - Update %i - Delete %i',10,1,'JobTopic', @AddCount_JBT, @UpdateCount_JBT, @DeleteCount_JBT) WITH NOWAIT;
END

