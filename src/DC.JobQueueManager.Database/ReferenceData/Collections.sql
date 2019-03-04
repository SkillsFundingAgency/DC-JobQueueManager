
BEGIN

	DECLARE @SummaryOfChanges_Collection TABLE ([CollectionId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[Collection] AS Target
	USING (
			SELECT NewRecords.[CollectionId], NewRecords.[Name], NewRecords.[IsOpen], CT.[CollectionTypeId], NewRecords.[CollectionYear]
			FROM 
			(
					  SELECT 1 AS [CollectionId], N'ILR1819' as [Name], 1 as [IsOpen], N'ILR' as [CollectionType], '1819' as [CollectionYear]
				UNION SELECT 2 AS [CollectionId], N'EAS1819' as [Name], 1 as [IsOpen], N'EAS' as [CollectionType], '1819' as [CollectionYear]
				UNION SELECT 3 AS [CollectionId], N'ESF'     as [Name], 1 as [IsOpen], N'ESF' as [CollectionType], '1819' as [CollectionYear]
				UNION SELECT 4 AS [CollectionId], N'ReferenceData1819'     as [Name], 0 as [IsOpen], N'REF' as [CollectionType], '1819' as [CollectionYear]
				UNION SELECT 5 AS [CollectionId], N'NCS1819'     as [Name], 1 as [IsOpen], N'NCS' as [CollectionType], '1819' as [CollectionYear]
				--UNION SELECT 4 AS [CollectionId], N'EAS1920' as [Name], 1 as [IsOpen], N'EAS' as [CollectionType], '1920' as [CollectionYear]
				--UNION SELECT 5 AS [CollectionId], N'ILR1920' as [Name], 1 as [IsOpen], N'ILR' as [CollectionType], '1920' as [CollectionYear]
			) AS NewRecords
			INNER JOIN [dbo].[CollectionType] CT
				ON CT.[TYPE] = NewRecords.[CollectionType]
		  )
		AS Source([CollectionId], [Name], [IsOpen], [CollectionTypeId], [CollectionYear])
		ON Target.[CollectionId] = Source.[CollectionId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[Name]
								  ,Target.[IsOpen]
								  ,Target.[CollectionTypeId]
								  ,Target.[CollectionYear]
						EXCEPT 
							SELECT Source.[Name]
								  ,Source.[IsOpen]
								  ,Source.[CollectionTypeId]
								  ,Source.[CollectionYear]
					)
			  THEN UPDATE SET Target.[Name] = Source.[Name],
							  Target.[IsOpen] = Source.[IsOpen],
							  Target.[CollectionTypeId] = Source.[CollectionTypeId],
							  Target.[CollectionYear] = Source.[CollectionYear]
		WHEN NOT MATCHED BY TARGET THEN INSERT([CollectionId], [IsOpen], [Name], [CollectionTypeId], [CollectionYear]) 
									   VALUES ([CollectionId], [IsOpen], [Name], [CollectionTypeId], [CollectionYear])
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[CollectionId],$action INTO @SummaryOfChanges_Collection([CollectionId],[Action])
	;

		DECLARE @AddCount_C INT, @UpdateCount_C INT, @DeleteCount_C INT
		SET @AddCount_C  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Collection WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_C = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Collection WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_C = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_Collection WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		         %s - Added %i - Update %i - Delete %i',10,1,'   Collection', @AddCount_C, @UpdateCount_C, @DeleteCount_C) WITH NOWAIT;
END
