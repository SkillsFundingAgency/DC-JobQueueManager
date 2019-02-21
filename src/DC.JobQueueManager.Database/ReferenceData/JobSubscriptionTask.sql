
BEGIN

	DECLARE @SummaryOfChanges_JobTopicTasks TABLE ([JobTopicTaskId] INT, [Action] VARCHAR(100));

	MERGE INTO [dbo].[JobSubscriptionTask] AS Target
	USING (VALUES
			( 1, 3,  N'TaskGenerateValidationReport', 1, 1),    -- ILR Stage 1
			( 2, 3,  N'TaskGenerateDataMatchReport', 1, 0),     -- ILR Stage 1
			( 3, 6,  N'ALB', 1, 1),                                             -- ILR Stage 2
			( 4, 6,  N'FM25', 2, 1),                                            -- ILR Stage 2
			( 5, 6,  N'FM35', 3, 1),                                            -- ILR Stage 2
			( 6, 6,  N'FM36', 4, 1),                                            -- ILR Stage 2
			( 7, 6,  N'FM70', 5, 1),                                            -- ILR Stage 2
			( 8, 6,  N'FM81', 7, 1),                                            -- ILR Stage 2
			( 9, 7,  N'PersistDataToDeds', 1, 1),                               -- ILR Stage 2
			(10, 8,  N'TaskGenerateDataMatchReport', 1, 0),                     -- ILR Stage 2
			(11, 8,  N'TaskGenerateValidationReport', 2, 1),                    -- ILR Stage 2
			(12, 8,  N'TaskGenerateAllbOccupancyReport', 3, 1),                 -- ILR Stage 2
			(13, 8,  N'TaskGenerateFundingSummaryReport', 4, 1),                -- ILR Stage 2
			(14, 8,  N'TaskGenerateMainOccupancyReport', 5, 1),                 -- ILR Stage 2
			(15, 8,  N'TaskGenerateMathsAndEnglishReport', 6, 1),               -- ILR Stage 2
			(16, 8,  N'TaskGenerateAppsAdditionalPaymentsReport', 7, 1),        -- ILR Stage 2
			(17, 8,  N'TaskGenerateAppsIndicativeEarningsReport', 8, 1),        -- ILR Stage 2
			(18, 8,  N'TaskGenerateTrailblazerEmployerIncentivesReport', 9, 1), -- ILR Stage 2
			(27, 8,  N'TaskGenerateFundingClaim1619Report', 10, 1),             -- ILR Stage 2
			(31, 8,  N'TaskGenerateTrailblazerAppsOccupancyReport', 11, 1),     -- ILR Stage 2

			(19, 9,  N'Validation', 1, 1),								-- EAS Submission : Process
			(20, 9,  N'Storage', 2, 1),									-- EAS Submission : Process
			(21, 9,  N'Reporting', 3, 1),								-- EAS Submission : Process
			(22, 10, N'TaskGenerateAdultFundingClaimReport', 1, 1),     -- EAS Submission : Reports
			(23, 10, N'TaskGenerateFundingSummaryReport', 2, 1),        -- EAS Submission : Reports

			(24, 11, N'Validation', 1, 1),								-- ESF Submission
			(25, 11, N'Storage', 2, 1),									-- ESF Submission
			(26, 11, N'Reporting', 3, 1),								-- ESF Submission

			(28, 13,  N'Fcs', 1, 0),   -- Ref Data : FCS
			(29, 14,  N'Epa', 1, 0),   -- Ref Data : EPA
			(30, 15,  N'Uln', 1, 0)	   -- Ref Data : Uln
		  )
		AS Source([JobTopicTaskId],[JobTopicId],[TaskName],[TaskOrder],[Enabled])
		ON Target.[JobTopicTaskId] = Source.[JobTopicTaskId]
		WHEN MATCHED 
				AND EXISTS 
					(		SELECT Target.[JobTopicTaskId] 
								  ,Target.[JobTopicId]
								  ,Target.[TaskName]
								  ,Target.[TaskOrder]
								  ,Target.[Enabled]
						EXCEPT 
							SELECT Source.[JobTopicTaskId]
								  ,Source.[JobTopicId]
								  ,Source.[TaskName]
								  ,Source.[TaskOrder]
								  ,Source.[Enabled]
					)
			  THEN UPDATE SET Target.[JobTopicId] = Source.[JobTopicId],
							  Target.[TaskName] = Source.[TaskName],
							  Target.[TaskOrder] = Source.[TaskOrder],
							  Target.[Enabled] = Source.[Enabled]

		WHEN NOT MATCHED BY TARGET THEN INSERT([JobTopicTaskId],[JobTopicId],[TaskName],[TaskOrder],[Enabled]) 
									   VALUES ([JobTopicTaskId],[JobTopicId],[TaskName],[TaskOrder],[Enabled])
		WHEN NOT MATCHED BY SOURCE THEN DELETE
		OUTPUT Inserted.[JobTopicTaskId],$action INTO @SummaryOfChanges_JobTopicTasks([JobTopicTaskId],[Action])
	;

		DECLARE @AddCount_JTT INT, @UpdateCount_JTT INT, @DeleteCount_JTT INT
		SET @AddCount_JTT  = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopicTasks WHERE [Action] = 'Insert' GROUP BY Action),0);
		SET @UpdateCount_JTT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopicTasks WHERE [Action] = 'Update' GROUP BY Action),0);
		SET @DeleteCount_JTT = ISNULL((SELECT Count(*) FROM @SummaryOfChanges_JobTopicTasks WHERE [Action] = 'Delete' GROUP BY Action),0);

		RAISERROR('		        %s - Added %i - Update %i - Delete %i',10,1,'JobTopic', @AddCount_JTT, @UpdateCount_JTT, @DeleteCount_JTT) WITH NOWAIT;
END

/*

SELECT
	JT.[JobTopicId]
	,X.[TaskName]
	,X.[TaskOrder]
	,X.[Enabled] 
	--,JT.*
	--,T.*
FROM 
(
      SELECT 1 as TaskOrder, 1 as [Enabled], N'Validation' as TaskName, N'Process'  as TopicName, N'EasSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 2 as TaskOrder, 1 as [Enabled], N'Storage' as TaskName, N'Process'  as TopicName, N'EasSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 3 as TaskOrder, 1 as [Enabled], N'Reporting' as TaskName, N'Process'  as TopicName, N'EasSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 1 as [Enabled], N'TaskGenerateAdultFundingClaimReport' as TaskName, N'Reports'  as TopicName, N'EasSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 2 as TaskOrder, 1 as [Enabled], N'TaskGenerateFundingSummaryReport' as TaskName, N'Reports'  as TopicName, N'EasSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 1 as [Enabled], N'Validation' as TaskName, N'Process'  as TopicName, N'EsfSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 2 as TaskOrder, 1 as [Enabled], N'Storage' as TaskName, N'Process'  as TopicName, N'EsfSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 3 as TaskOrder, 1 as [Enabled], N'Reporting' as TaskName, N'Process'  as TopicName, N'EsfSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 1 as [Enabled], N'TaskGenerateValidationReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 0 as [Enabled], N'TaskGenerateDataMatchReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 1 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 1 as [Enabled], N'PersistDataToDeds' as TaskName, N'Deds'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 1 as [Enabled], N'ALB' as TaskName, N'Funding'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 2 as TaskOrder, 1 as [Enabled], N'FM25' as TaskName, N'Funding'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 3 as TaskOrder, 1 as [Enabled], N'FM35' as TaskName, N'Funding'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 4 as TaskOrder, 1 as [Enabled], N'FM36' as TaskName, N'Funding'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 5 as TaskOrder, 1 as [Enabled], N'FM70' as TaskName, N'Funding'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 7 as TaskOrder, 1 as [Enabled], N'FM81' as TaskName, N'Funding'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 1 as TaskOrder, 0 as [Enabled], N'TaskGenerateDataMatchReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 2 as TaskOrder, 1 as [Enabled], N'TaskGenerateValidationReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 3 as TaskOrder, 1 as [Enabled], N'TaskGenerateAllbOccupancyReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 4 as TaskOrder, 1 as [Enabled], N'TaskGenerateFundingSummaryReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 5 as TaskOrder, 1 as [Enabled], N'TaskGenerateMainOccupancyReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 6 as TaskOrder, 1 as [Enabled], N'TaskGenerateMathsAndEnglishReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 7 as TaskOrder, 1 as [Enabled], N'TaskGenerateAppsAdditionalPaymentsReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 8 as TaskOrder, 1 as [Enabled], N'TaskGenerateAppsIndicativeEarningsReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 9 as TaskOrder, 1 as [Enabled], N'TaskGenerateTrailblazerEmployerIncentivesReport' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 
UNION SELECT 10 as TaskOrder, 1 as [Enabled], N'TaskGenerateFundingClaim1619Report' as TaskName, N'Reports'  as TopicName, N'IlrSubmission'  as JobType, 0 as IsFirstStage 

) as X
INNER JOIN [dbo].[JobType] T
	ON T.[Title] = X.[JobType]
INNER JOIN [dbo].[JobTopic] JT 
	 ON JT.[TopicName] = X.[TopicName]
	AND ISNULL(JT.[IsFirstStage],-99) = X.[IsFirstStage]
	AND T.[JobTypeId]= JT.[JobTypeId]
--ORDER BY T.[JobTypeGroupId],X.[JobType],X.[IsFirstStage] DESC, X.[TopicName], X.[TaskOrder]
--INTERSECT 
--EXCEPT
SELECT --[JobTopicTaskId],
[JobTopicId],[TaskName],[TaskOrder],[Enabled]  FROM  [dbo].[JobTopicTask] 



 --SELECT * FROM [dbo].[JobType] 
-- SELECT * FROM [dbo].[JobTopic] 


SELECT 'UNION SELECT ' --+ CONVERT(VARCHAR,JTT.[JobTopicId]) + ' as JobTopicId,  ' 
		  + CONVERT(VARCHAR,JTT.[TaskOrder]) + ' as TaskOrder, ' 
		  + CONVERT(VARCHAR,JTT.[Enabled]) + ' as [Enabled], '
		  + 'N''' + JTT.[TaskName] + ''' as TaskName, '
		  + 'N''' + JT.[TopicName] + '''  as TopicName, '
		  + 'N''' + T.[Title] + '''  as JobType, '
		  + CONVERT(VARCHAR,ISNULL(JT.[IsFirstStage],-99)) + ' as IsFirstStage '
		  --,T.*
FROM [dbo].[JobTopicTask] JTT
INNER JOIN [dbo].[JobTopic] JT 
	ON JT.[JobTopicId] = JTT.[JobTopicId]
INNER JOIN [dbo].[JobType] T
	ON T.[JobTypeId] = JT.[JobTypeId]
ORDER BY T.[JobTypeGroupId], T.[Title],JT.[IsFirstStage] DESC, JT.[TopicName], JTT.[TaskOrder]

*/

