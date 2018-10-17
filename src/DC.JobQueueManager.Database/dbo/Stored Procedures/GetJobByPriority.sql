
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
	AND 
	(
		jt.ProcessingOverrideFlag = 1 
		OR
		(
			jt.ProcessingOverrideFlag IS NULL 
			AND	Exists (select 1 from ReturnPeriod rp Where CollectionId = c.CollectionId And
				j.DateTimeSubmittedUTC between rp.StartDateTimeUTC AND rp.EndDateTimeUTC)
		)
	)
	
	AND NOT EXISTS (SELECT 1 FROM [dbo].[Job] j1  (nolock) 
					 LEFT JOIN dbo.FileUploadJobMetaData meta1 WITH (NOLOCK)
						ON j1.JobId = meta1.JobId
					WHERE [Status] IN (2,3) 
					  And ( [JobType] = 4  Or ([JobType] In (1,2,3) And meta.[Ukprn] = meta1.[Ukprn]) ))
					
	ORDER BY [Priority] DESC, j.JobId

END

GO

--GRANT EXECUTE
--    ON OBJECT::[dbo].[GetJobByPriority] TO [JobQueueManagerSchedulerUser]
--    AS [dbo];

GO

GRANT EXECUTE
    ON OBJECT::[dbo].[GetJobByPriority] TO [JobManagementSchedulerUser]
    AS [dbo];

GO
