
-- =============================================
-- Author:      Name
-- Create Date: 
-- Description: 
-- =============================================
CREATE PROCEDURE GetJobByPriority
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT TOP 1 
		   [JobId]
		  ,[JobType]
		  ,[Priority]
		  ,[DateTimeSubmittedUTC]
		  ,[DateTimeUpdatedUTC]
		  ,[Ukprn]
		  ,[Status]
		  ,[Rowversion]
		  ,[SubmittedBy]
	FROM [dbo].[job] j WITH (nolock) 
	WHERE [Status] = 1
	AND NOT EXISTS (SELECT 1 FROM [dbo].[job] (nolock) 
					WHERE [Status] IN (5,7) 
					  And ( [JobType] = 2  Or ([JobType] =1 And [Ukprn] = j.[Ukprn]) )
					)
	ORDER BY [Priority] DESC, [JobId]

END

GO

GRANT EXECUTE
    ON OBJECT::[dbo].[GetJobByPriority] TO [JobQueueManagerApiUser]
    AS [dbo];

GO

