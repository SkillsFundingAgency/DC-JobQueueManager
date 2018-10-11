
BEGIN

	DECLARE @SummaryOfChanges_CollectionType TABLE ([CollectionTypeId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[CollectionType] AS Target
	USING (VALUES
			(1, N'ILR', N'ILR (Individualised Learner Records)'),
			(2, N'EAS', N'EAS (Earnings Adjustment Statement)'),
			(3, N'ESF', N'ESF (European Social Fund) supplementary data')
		  )
		AS Source([CollectionTypeId], [Type], [Description])
		ON Target.[CollectionTypeId] = Source.[CollectionTypeId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[Description]
								  ,Target.[Type]
						EXCEPT 
							SELECT Source.[Description]
								  ,Source.[Type]
					)
			  THEN UPDATE SET Target.[Description] = Source.[Description],
							  Target.[Type] = Source.[Type]
		WHEN NOT MATCHED BY TARGET THEN INSERT([CollectionTypeId], [Type], [Description]) 
									   VALUES ([CollectionTypeId], [Type], [Description])
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[CollectionTypeId],$action INTO @SummaryOfChanges_CollectionType([CollectionTypeId],[Action])
	;

		DECLARE @AddCount_CT INT, @UpdateCount_CT INT, @DeleteCount_CT INT
		SET @AddCount_CT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_CollectionType WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_CT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_CollectionType WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_CT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_CollectionType WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'CollectionType', @AddCount_CT, @UpdateCount_CT, @DeleteCount_CT) WITH NOWAIT;
END
