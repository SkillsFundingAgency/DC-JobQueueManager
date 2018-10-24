
DECLARE @SummaryOfChanges_JobEmailTemplate TABLE ([EventId] varchar(500), [Action] VARCHAR(100));

MERGE INTO [dbo].[JobEmailTemplate] AS Target
USING (VALUES
		('3cfbfb6b-0a8e-48f1-b716-268af491696b','90a341c2-dcf2-41b7-87c7-4e341f02616d',1,1,1), --ILR
		('e2219426-4cd8-4bb6-9f96-f77ea040699a',NULL,4,1,1),--ILR

		('0cff79db-9e77-4f67-aa37-5d10752751f3',NULL,1,1,2),--EAS
		('8ad7d223-fae9-4b37-a5e5-9aaa13fdc7c5',NULL,4,1,2),--EAS

		('78237b46-7602-454f-9c3d-ec2601554909',NULL,1,1,3),--ESF
		('61c0129b-f03d-4e7f-b188-4dc59293cace',NULL,4,1,3)--ESF
	  )
	AS Source([TemplateOpenPeriod],[TemplateClosePeriod], [JobStatus], [Active],[JobType])
	ON Target.[TemplateOpenPeriod] = Source.[TemplateOpenPeriod]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[JobStatus]
							  ,Target.[Active]
							  ,Target.[JobType]
							  ,Target.[TemplateOpenPeriod]
							  ,Target.[TemplateClosePeriod]
					EXCEPT 
						SELECT source.[JobStatus]
							  ,source.[Active]
							  ,source.[JobType]
							  ,source.[TemplateOpenPeriod]
							  ,source.[TemplateClosePeriod]
				)
		  THEN UPDATE SET Target.[JobStatus] = Source.[JobStatus],
			              Target.[Active] = Source.[Active],
						  Target.[JobType] = Source.[JobType],
						  Target.[TemplateOpenPeriod] = source.[TemplateOpenPeriod],
						  Target.[TemplateClosePeriod] = source.[TemplateClosePeriod]
	WHEN NOT MATCHED BY TARGET THEN INSERT([TemplateOpenPeriod],[TemplateClosePeriod], [JobStatus], [Active],[JobType]) 
								   VALUES ([TemplateOpenPeriod],[TemplateClosePeriod], [JobStatus], [Active],[JobType])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT Inserted.[TemplateOpenPeriod],$action INTO @SummaryOfChanges_JobEmailTemplate([EventId],[Action])
;

	DECLARE @AddCount_JET INT, @UpdateCount_JET INT, @DeleteCount_JET INT
	SET @AddCount_JET  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobEmailTemplate WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_JET = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobEmailTemplate WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_JET = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobEmailTemplate WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'JobEmailTemplate', @AddCount_JET, @UpdateCount_JET, @DeleteCount_JET) WITH NOWAIT;

