
BEGIN

	DECLARE @SummaryOfChanges_JobTopic TABLE ([JobTopicId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[JobTopicSubscription] AS Target
	USING (VALUES
			-- ILR Data
			(1,  1, 1, N'FileValidation', N'ilr1819submissiontopic', 1, 1, 1),
			(2,  1, 1,  N'Validation', N'ilr1819submissiontopic', 2, 1, 1),
			(3,  1, 1, N'Reports', N'ilr1819submissiontopic', 3, 1, 1),
			(4,  1, 1,  N'FileValidation', N'ilr1819submissiontopic', 1, 0, 1),
			(5,  1, 1, N'Validation', N'ilr1819submissiontopic',  2, 0, 1),
			(6,  1, 1, N'Funding', N'ilr1819submissiontopic',  3, 0, 1),
		   (12,  1, 1,  N'GenerateFM36Payments', N'ilr1819submissiontopic', 4, 0, 0),
			(7,  1, 1,  N'Deds', N'ilr1819submissiontopic', 5, 0, 1),
			(8,  1, 1, N'Reports', N'ilr1819submissiontopic',  6, 0, 1),

			-- EAS Submission
			(9,  2, 2, N'Process', N'eas1819submissiontopic',  1, null, 1),
			(10, 2, 2,  N'Reports', N'eas1819submissiontopic', 1, null, 1),

			-- ESF Submission
			(11, 3, 3,  N'Process', N'esfv1submissiontopic',1, null, 1),

			-- Reference Data
			(13, 40, 4, N'Process', N'referencedatatopic',  1, null, 0),
			(14, 41, 4, N'Process', N'referencedatatopic',  1, null, 0),
			(15, 42, 4, N'Process', N'referencedatatopic',  1, null, 0)
		  )
		AS Source([JobTopicId],[JobTypeId],[CollectionId],[SubscriptionName],[TopicName],[TopicOrder],[IsFirstStage],[Enabled] )
		ON Target.[JobTopicId] = Source.[JobTopicId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[JobTopicId]
								  ,Target.[JobTypeId]
								  ,Target.[CollectionId]
								  ,Target.[SubscriptionName]
								  ,Target.[TopicName]
								  ,Target.[TopicOrder]
								  ,Target.[IsFirstStage]
								  ,Target.[Enabled]
						EXCEPT 
							SELECT Source.[JobTopicId]
								  ,Source.[JobTypeId]
								  ,Source.[CollectionId]
								  ,Source.[SubscriptionName]
								  ,Source.[TopicName]
								  ,Source.[TopicOrder]
								  ,Source.[IsFirstStage]
								  ,Source.[Enabled]
					)
			  THEN UPDATE SET Target.[JobTypeId] = Source.[JobTypeId],
							  Target.[CollectionId] = Source.[CollectionId],
							  Target.[SubscriptionName] = Source.[SubscriptionName],
							  Target.[TopicName] = Source.[TopicName],
							  Target.[TopicOrder] = Source.[TopicOrder],
							  Target.[IsFirstStage] = Source.[IsFirstStage],
							  Target.[Enabled] = Source.[Enabled]

		WHEN NOT MATCHED BY TARGET THEN INSERT([JobTopicId],[JobTypeId],[CollectionId],[SubscriptionName],[TopicName],[TopicOrder],[IsFirstStage],[Enabled] ) 
									   VALUES ([JobTopicId],[JobTypeId],[CollectionId],[SubscriptionName],[TopicName],[TopicOrder],[IsFirstStage],[Enabled] )
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[JobTopicId],$action INTO @SummaryOfChanges_JobTopic([JobTopicId],[Action])
	;

		DECLARE @AddCount_JBT INT, @UpdateCount_JBT INT, @DeleteCount_JBT INT
		SET @AddCount_JBT  = ISNULL((SELECT COUNT(*) FROM @SummaryOfChanges_JobTopic WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_JBT = ISNULL((SELECT COUNT(*) FROM @SummaryOfChanges_JobTopic WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_JBT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopic WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		        %s - Added %i - Update %i - Delete %i',10,1,'JobTopic', @AddCount_JBT, @UpdateCount_JBT, @DeleteCount_JBT) WITH NOWAIT;
END

