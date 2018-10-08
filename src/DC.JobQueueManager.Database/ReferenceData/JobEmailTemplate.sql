
DECLARE @SummaryOfChanges_JobEmailTemplate TABLE ([EventId] varchar(500), [Action] VARCHAR(100));

MERGE INTO [dbo].[JobEmailTemplate] AS Target
USING (VALUES
		('3cfbfb6b-0a8e-48f1-b716-268af491696b',2,1,1),
		('e2219426-4cd8-4bb6-9f96-f77ea040699a',4,1,1)
	  )
	AS Source([TemplateId], [JobStatus], [Active],[JobType])
	ON Target.[TemplateId] = Source.[TemplateId]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[JobStatus]
							  ,Target.[Active]
							  ,Target.[JobType]
					EXCEPT 
						SELECT source.[JobStatus]
							  ,source.[Active]
							  ,source.[JobType]
				)
		  THEN UPDATE SET Target.[JobStatus] = Source.[JobStatus],
			              Target.[Active] = Source.[Active],
						  Target.[JobType] = Source.[JobType]
	WHEN NOT MATCHED BY TARGET THEN INSERT([TemplateId], [JobStatus], [Active],[JobType]) 
								   VALUES ([TemplateId], [JobStatus], [Active],[JobType])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT Inserted.[TemplateId],$action INTO @SummaryOfChanges_JobEmailTemplate([EventId],[Action])
;

	DECLARE @AddCount_JET INT, @UpdateCount_JET INT, @DeleteCount_JET INT
	SET @AddCount_JET  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobEmailTemplate WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_JET = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobEmailTemplate WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_JET = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobEmailTemplate WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'JobEmailTemplate', @AddCount_JET, @UpdateCount_JET, @DeleteCount_JET) WITH NOWAIT;

