CREATE VIEW [dbo].[vw_JobInfo]
AS 

SELECT FUJMD.[Ukprn]
      ,J.[JobId]
      ,JT.[Description]
      ,J.[JobType]
      ,J.[Priority]
      ,J.[DateTimeSubmittedUTC]
      ,J.[DateTimeUpdatedUTC]
	  ,DATEDIFF(SS,[DateTimeSubmittedUTC],[DateTimeUpdatedUTC]) as TimeDiff_Seconds
      ,J.[SubmittedBy]
      ,JST.[StatusDescription]
      ,J.[Status]
      --,J.[RowVersion]
      --,J.[NotifyEmail]
	  ,FUJMD.[FileName]
      ,FUJMD.[FileSize]
      ,FUJMD.[StorageReference]
      ,FUJMD.[IsFirstStage]
      ,FUJMD.[CollectionName]
      ,FUJMD.[PeriodNumber]
      --,FUJMD.[TermsAccepted]
      ,J.[CrossLoadingStatus]
FROM [dbo].[Job] AS J
INNER JOIN [dbo].[JobStatusType] AS JST
   ON JST.[StatusId] = J.[Status]
INNER JOIN [dbo].[JobType] AS JT
   ON JT.[JobTypeId] = J.[JobType]
LEFT JOIN [dbo].[FileUploadJobMetaData] FUJMD
	ON J.[JobId] = FUJMD.[JobId]

