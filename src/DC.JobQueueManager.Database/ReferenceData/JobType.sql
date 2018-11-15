
DECLARE @SummaryOfChanges_JobType TABLE ([EventId] INT, [Action] VARCHAR(100));

MERGE INTO [dbo].[JobType] AS Target
USING (VALUES
		(1,'IlrSubmission','IlrSubmission', 1),
		(2,'EasSubmission','EasSubmission', 1),
		(3,'EsfSubmission','EsfSubmission', 1),		
		(20,'PeriodEnd','PeriodEnd', 2),
		(40,'ReferenceData EPA','ReferenceData EPA', 3),
		(41,'ReferenceData FCS','ReferenceData FCS', 3)
	  )
	AS Source([JobTypeId], [Title], [Description], [JobTypeGroupId])
	ON Target.[JobTypeId] = Source.[JobTypeId]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[JobTypeId]
					EXCEPT 
						SELECT Source.[JobTypeId]
				)
		  THEN UPDATE SET Target.[JobTypeId] = Source.[JobTypeId],
						  Target.[Description] = Source.[Description],
			              Target.[Title] = Source.[Title],
						  Target.[JobTypeGroupId] = Source.[JobTypeGroupId]
	WHEN NOT MATCHED BY TARGET THEN INSERT([JobTypeId], [Title], [Description], [JobTypeGroupId]) 
								   VALUES ([JobTypeId], [Title], [Description], [JobTypeGroupId])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT Inserted.[JobTypeId],$action INTO @SummaryOfChanges_JobType([EventId],[Action])
;

	DECLARE @AddCount_JT INT, @UpdateCount_JT INT, @DeleteCount_JT INT
	SET @AddCount_JT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobType WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_JT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobType WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_JT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobType WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		               %s - Added %i - Update %i - Delete %i',10,1,'JobType', @AddCount_JT, @UpdateCount_JT, @DeleteCount_JT) WITH NOWAIT;

