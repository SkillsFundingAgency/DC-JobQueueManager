
BEGIN

	DECLARE @SummaryOfChanges_Collection TABLE ([CollectionId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[Collection] AS Target
	USING (
			SELECT NewRecords.[CollectionId], NewRecords.[Name], NewRecords.[IsOpen], CT.[CollectionTypeId]
			FROM 
			(
				  SELECT 1 AS [CollectionId], N'ILR1819' as [Name], 1 as [IsOpen], N'ILR' as [CollectionType]
			UNION SELECT 2 AS [CollectionId], N'EAS' as [Name],     1 as [IsOpen], N'EAS' as [CollectionType]
			UNION SELECT 3 AS [CollectionId], N'ESF' as [Name],     1 as [IsOpen], N'ESF' as [CollectionType]
			) AS NewRecords
			INNER JOIN [dbo].[CollectionType] CT
				ON CT.[TYPE] = NewRecords.[CollectionType]
		  )
		AS Source([CollectionId], [Name], [IsOpen], [CollectionTypeId])
		ON Target.[CollectionId] = Source.[CollectionId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[Name]
								  ,Target.[IsOpen]
								  ,Target.[CollectionTypeId]
						EXCEPT 
							SELECT Source.[Name]
								  ,Source.[IsOpen]
								  ,Source.[CollectionTypeId]
					)
			  THEN UPDATE SET Target.[Name] = Source.[Name],
							  Target.[IsOpen] = Source.[IsOpen],
							  Target.[CollectionTypeId] = Source.[CollectionTypeId]
		WHEN NOT MATCHED BY TARGET THEN INSERT([CollectionId], [IsOpen], [Name], [CollectionTypeId]) 
									   VALUES ([CollectionId], [IsOpen], [Name], [CollectionTypeId])
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[CollectionId],$action INTO @SummaryOfChanges_Collection([CollectionId],[Action])
	;

		DECLARE @AddCount_C INT, @UpdateCount_C INT, @DeleteCount_C INT
		SET @AddCount_C  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Collection WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_C = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Collection WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_C = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Collection WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		      %s - Added %i - Update %i - Delete %i',10,1,'    Collection', @AddCount_C, @UpdateCount_C, @DeleteCount_C) WITH NOWAIT;
END
