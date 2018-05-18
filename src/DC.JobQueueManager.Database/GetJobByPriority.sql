
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

	Select Top 1 * From
	dbo.job j with (nolock) 
	Where Status = 1
	And NOT Exists (Select 1 from job WITH (NOLOCK) where Status IN (5,7) And (JobType = 2 Or (JobType =1 And Ukprn = j.Ukprn )))
	Order by Priority desc, JobId

END

