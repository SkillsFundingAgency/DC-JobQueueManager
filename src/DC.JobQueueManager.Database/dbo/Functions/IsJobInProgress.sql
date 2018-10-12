
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
--DROp FUNCTION [dbo].[IsJobInProgress]
CREATE FUNCTION [dbo].[IsJobInProgress]
(
	-- Add the parameters for the function here
	@ukprn bigint
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result bit = 0

	if(@ukprn is null)
		Return @result

	--check if another job is in progress for same ukprn
	If EXISTS (SELECT 1 FROM [dbo].[Job] j  (nolock) 
					 INNER JOIN dbo.FileUploadJobMetaData meta WITH (NOLOCK)
						ON j.JobId = meta.JobId
					WHERE [Status] IN (2,3) And  @Ukprn = meta.[Ukprn])
		Set @Result = 1

	-- Return the result of the function
	RETURN @Result

END