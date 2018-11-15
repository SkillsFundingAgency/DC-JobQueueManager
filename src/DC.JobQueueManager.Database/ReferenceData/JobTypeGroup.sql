
DECLARE @SummaryOfChanges_JobTypeGroup TABLE ([EventId] INT, [Action] VARCHAR(100));

MERGE INTO [dbo].[JobTypeGroup] AS Target
USING (VALUES
		(1, 'Collection Submission', 25),
		(2, 'Period End', 25),
		(3, 'Reference Data', 25)
	  )
	AS Source([JobTypeGroupId], [Description], [ConcurrentExecutionCount])
	ON Target.[JobTypeGroupId] = Source.[JobTypeGroupId]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[JobTypeGroupId]
					EXCEPT 
						SELECT Source.[JobTypeGroupId]
				)
		  THEN UPDATE SET Target.[JobTypeGroupId] = Source.[JobTypeGroupId],
			              Target.[Description] = Source.[Description],
						  Target.[ConcurrentExecutionCount] = Source.[ConcurrentExecutionCount]
	WHEN NOT MATCHED BY TARGET THEN INSERT([JobTypeGroupId], [Description], [ConcurrentExecutionCount]) 
								   VALUES ([JobTypeGroupId], [Description], [ConcurrentExecutionCount])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT Inserted.[JobTypeGroupId],$action INTO @SummaryOfChanges_JobTypeGroup([EventId],[Action])
;

	DECLARE @AddCount_JT INT, @UpdateCount_JT INT, @DeleteCount_JT INT
	SET @AddCount_JT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTypeGroup WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_JT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTypeGroup WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_JT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTypeGroup WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		               %s - Added %i - Update %i - Delete %i',10,1,'JobType', @AddCount_JT, @UpdateCount_JT, @DeleteCount_JT) WITH NOWAIT;

