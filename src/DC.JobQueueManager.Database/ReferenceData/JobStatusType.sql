

DECLARE @SummaryOfChanges_JobStatusType TABLE ([EventId] INT, [Action] VARCHAR(100));

MERGE INTO [dbo].[JobStatusType] AS Target
USING (VALUES
		(1,'Ready','Ready'),
		(2,'MovedForProcessing','Moved For Processing'),
		(3,'Processing','Processing'),
		(4,'Completed','Completed'),
		(5,'FailedRetry','Failed Retry'),
		(6,'Failed','Failed'),
		(7,'Paused','Paused'),
		(8,'Waiting','Waiting')
	  )
	AS Source([StatusId], [StatusTitle], [StatusDescription])
	ON Target.[StatusId] = Source.[StatusId]
	WHEN MATCHED 
			AND EXISTS 
				(		SELECT Target.[StatusDescription]
							  ,Target.[StatusTitle]
					EXCEPT 
						SELECT Source.[StatusDescription]
						      ,Source.[StatusTitle]
				)
		  THEN UPDATE SET Target.[StatusDescription] = Source.[StatusDescription],
			              Target.[StatusTitle] = Source.[StatusTitle]
	WHEN NOT MATCHED BY TARGET THEN INSERT([StatusId], [StatusTitle], [StatusDescription]) 
								   VALUES ([StatusId], [StatusTitle], [StatusDescription])
	WHEN NOT MATCHED BY SOURCE THEN DELETE
	OUTPUT Inserted.[StatusId],$action INTO @SummaryOfChanges_JobStatusType([EventId],[Action])
;

	DECLARE @AddCount_JST INT, @UpdateCount_JST INT, @DeleteCount_JST INT
	SET @AddCount_JST  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobStatusType WHERE [Action] = 'Insert' GROUP BY Action),0);
	SET @UpdateCount_JST = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobStatusType WHERE [Action] = 'Update' GROUP BY Action),0);
	SET @DeleteCount_JST = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobStatusType WHERE [Action] = 'Delete' GROUP BY Action),0);

	RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'JobStatusType', @AddCount_JST, @UpdateCount_JST, @DeleteCount_JST) WITH NOWAIT;

