
-- =============================================
-- Author:      Name
-- Create Date: 
-- Description: 
-- =============================================
CREATE PROCEDURE [dbo].[GetJobByPriority]
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT TOP 1 
		   j.JobId
		  ,[JobType]
		  ,[Priority]
		  ,[DateTimeSubmittedUTC]
		  ,[DateTimeUpdatedUTC]
		  ,[Ukprn]
		  ,[Status]
		  ,[RowVersion]
		  ,[SubmittedBy]
		  ,[NotifyEmail]
		  ,[CrossLoadingStatus]
	FROM [dbo].[Job] j WITH (nolock) 
	INNER JOIN [dbo].[JobType] jt WITH (nolock) 
		on jt.JobTypeId = j.JobType
	LEFT JOIN dbo.FileUploadJobMetaData meta WITH (NOLOCK)
		ON j.JobId = meta.JobId
	LEFT JOIN dbo.[Collection] c ON c.[Name] = meta.CollectionName

	WHERE [Status] = 1
	AND IsNull(c.IsOpen,1) = 1 
	AND dbo.CanProcessJob(c.CollectionId,j.DateTimeSubmittedUTC,j.JobType,meta.IsFirstStage) = 1
	AND dbo.IsJobInProgress(meta.Ukprn) = 0
					
	ORDER BY [Priority] DESC, j.JobId

END

GO

GRANT EXECUTE
    ON OBJECT::[dbo].[GetJobByPriority] TO [JobQueueManagerSchedulerUser]
    AS [dbo];

GO

