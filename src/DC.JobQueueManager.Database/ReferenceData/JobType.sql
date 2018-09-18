
DECLARE @SummaryOfChanges_JobType TABLE ([EventId] INT, [Action] VARCHAR(100));

MERGE INTO [dbo].[JobType] AS Target
USING (VALUES
		(1,'IlrSubmission','IlrSubmission'),
		(2,'EasSubmission','EasSubmission'),
		(3,'EsfSubmission','EsfSubmission'),
		(4,'ReferenceData','ReferenceData'),
		(5,'PeriodEnd','PeriodEnd')
		
	  )
	AS Source([JobTypeId], [Title], [Description])
	ON Target.[JobTypeId] = Source.[JobTypeId]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[Description]
							  ,Target.[Title]
					EXCEPT 
						SELECT Source.[Description]
						      ,Source.[Title]
				)
		  THEN UPDATE SET Target.[Description] = Source.[Description],
			              Target.[Title] = Source.[Title]
	WHEN NOT MATCHED BY TARGET THEN INSERT([JobTypeId], [Title], [Description]) 
								   VALUES ([JobTypeId], [Title], [Description])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT Inserted.[JobTypeId],$action INTO @SummaryOfChanges_JobType([EventId],[Action])
;

	DECLARE @AddCount_JT INT, @UpdateCount_JT INT, @DeleteCount_JT INT
	SET @AddCount_JT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobType WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_JT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobType WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_JT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobType WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'JobType', @AddCount_JT, @UpdateCount_JT, @DeleteCount_JT) WITH NOWAIT;

