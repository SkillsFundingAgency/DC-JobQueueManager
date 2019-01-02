CREATE VIEW [dbo].[vw_DisplayJobTaskList]
AS	
	SELECT TOP 100 PERCENT 
			JG.[Description] as JobGroup
		   ,JG.[ConcurrentExecutionCount] as [JobGroupConcurrentExecutionCount]
		   ,J.[Title] as JobType
		   ,JT.[TopicName] as Topic
		   ,JT.[Enabled] as TopicEnabled
		   ,JTT.[TaskName] as TaskName
		   ,JTT.[Enabled] as TaskEnabled
		   ,ISNULL(JT.[IsFirstStage],0) as [IsFirstStage]
		   ,J.[IsCrossLoadingEnabled] as [IsCrossLoadingEnabled]
		   ,[ProcessingOverrideFlag]
		  --,JG.*
		  --,J.*
		  --,JT.*
		  --,JTT.*
	FROM [dbo].[JobTopicTask] JTT
	INNER JOIN [dbo].[JobTopic] JT 
		ON JT.[JobTopicId] = JTT.[JobTopicId]
	INNER JOIN [dbo].[JobType] J
		ON J.[JobTypeId] = JT.[JobTypeId]
	LEFT JOIN [dbo].[JobTypeGroup] JG
		ON JG.[JobTypeGroupId] = J.[JobTypeGroupId]
	ORDER BY 
		 ISNULL(JG.[JobTypeGroupId],0) ASC
		,J.[JobTypeId] ASC
		,ISNULL(JT.[IsFirstStage],0) DESC
		,JT.[TopicOrder] ASC
		,JTT.[TaskOrder] ASC

  