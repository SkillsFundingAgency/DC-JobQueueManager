
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
		  ,[IsCorssLoaded]
	FROM [dbo].[Job] j WITH (nolock) 
	LEFT JOIN dbo.FileUploadJobMetaData meta WITH (NOLOCK)
		ON j.JobId = meta.JobId
	WHERE [Status] = 1
	AND NOT EXISTS (SELECT 1 FROM [dbo].[Job] j1  (nolock) 
					 LEFT JOIN dbo.FileUploadJobMetaData meta1 WITH (NOLOCK)
						ON j1.JobId = meta1.JobId
					WHERE [Status] IN (2,3) 
					  And ( [JobType] = 4  Or ([JobType] In (1,2,3) And meta.[Ukprn] = meta1.[Ukprn]) )
					)
	ORDER BY [Priority] DESC, j.JobId

END

GO

GRANT EXECUTE
    ON OBJECT::[dbo].[GetJobByPriority] TO [JobQueueManagerSchedulerUser]
    AS [dbo];

GO

