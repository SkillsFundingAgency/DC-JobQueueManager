
-- =============================================
-- Author:      Name
-- Create Date: 
-- Description: 
-- =============================================
CREATE PROCEDURE [dbo].[GetJobByPriority]
	@ResultCount int
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	-- The table structure returned to the caller
	Declare @ReturnResults Table (
		[JobId] bigint,
		[JobType] smallint,
		[Priority] smallint,
		[DateTimeSubmittedUTC] datetime,
		[DateTimeUpdatedUTC] datetime,
		[Status] smallint,
		[RowVersion] binary(8),
		[SubmittedBy] varchar(50),
		[NotifyEmail] nvarchar(500),
		[CrossLoadingStatus] smallint
	)

	-- Check for active reference data jobs
	Insert into @ReturnResults ([JobId], [JobType], [Priority], [DateTimeSubmittedUTC], [DateTimeUpdatedUTC], [Status], [RowVersion], [SubmittedBy], [NotifyEmail], [CrossLoadingStatus])
	Select j.JobId
		  ,[JobType]
		  ,[Priority]
		  ,[DateTimeSubmittedUTC]
		  ,[DateTimeUpdatedUTC]
		  ,[Status]
		  ,[RowVersion]
		  ,[SubmittedBy]
		  ,[NotifyEmail]
		  ,[CrossLoadingStatus]
	FROM [dbo].[Job] j WITH (nolock) 
	WHERE [Status] = 1
	AND [JobType] = 4

	if @@RowCount = 0
	begin
		-- Check for any running ref data jobs that prevent us from running normal jobs
		Declare @NumRefJobsRunning int = 0
		Select @NumRefJobsRunning = Count(JobId)
		from [dbo].[Job]
		WHERE [Status] IN (2,3)
		AND [JobType] = 4

		if @NumRefJobsRunning = 0
		begin
			-- Get all currently running Ukprns so that we don't try and create them as jobs
			Declare @RunningUkprns Table (
				[Ukprn] bigint
			)

			Insert into @RunningUkprns ([Ukprn])
			Select meta.[Ukprn]
			From [dbo].[Job] j  (nolock) 
			INNER JOIN dbo.FileUploadJobMetaData meta WITH (NOLOCK)
			ON j.JobId = meta.JobId
			WHERE j.[Status] IN (2,3)

			-- If not reference data jobs, then look for normal jobs. Use row_number as protection from running the same queued Ukprn 2+ times.
			Insert into @ReturnResults ([JobId], [JobType], [Priority], [DateTimeSubmittedUTC], [DateTimeUpdatedUTC], [Status], [RowVersion], [SubmittedBy], [NotifyEmail], [CrossLoadingStatus])
			Select [JobId], [JobType], [Priority], [DateTimeSubmittedUTC], [DateTimeUpdatedUTC], [Status], [RowVersion], [SubmittedBy], [NotifyEmail], [CrossLoadingStatus]
			from (
				SELECT TOP 100 
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
					  ,[CrossLoadingStatus],
					  ROW_NUMBER() OVER(PARTITION BY [Ukprn] ORDER BY [Priority] DESC) rn
				FROM [dbo].[Job] j WITH (nolock) 
				INNER JOIN [dbo].[JobType] jt WITH (nolock) 
					on jt.JobTypeId = j.JobType
				LEFT JOIN dbo.FileUploadJobMetaData meta WITH (NOLOCK)
					ON j.JobId = meta.JobId
				LEFT JOIN dbo.[Collection] c ON c.[Name] = meta.CollectionName

				WHERE j.[Status] = 1
				AND IsNull(c.IsOpen, 1) = 1 
				AND dbo.CanProcessJob(c.CollectionId, j.DateTimeSubmittedUTC, j.JobType, meta.IsFirstStage) = 1			
			) a
			Where rn = 1 AND [UKPRN] NOT IN (Select Ukprn from @RunningUkprns)					
			ORDER BY [Priority] DESC, JobId
		End
	End

	Select Top (@ResultCount) [JobId], [JobType], [Priority], [DateTimeSubmittedUTC], [DateTimeUpdatedUTC], [Status], [RowVersion], [SubmittedBy], [NotifyEmail], [CrossLoadingStatus]
	From @ReturnResults

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
