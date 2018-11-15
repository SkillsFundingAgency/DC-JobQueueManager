--	 ,=\.-----""""^==--
--	;;'( ,___, ,/~\;                  
--	'  )/>/   \|-,                  
--	   | `\    | "                  
--	   "   "   "  
-- TODO: Period end
CREATE PROCEDURE [dbo].[GetJobByPriority]
	@ResultCount int
AS
BEGIN
    SET NOCOUNT ON

	Declare @ConstRefDataGroup int = 3
	Declare @GroupToFilter int = -1
	Declare @ConcurrentExecutionCount int = 0
	Declare @NumJobsRunning int = 0

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

	-- Check for any running reference data jobs that prevent us from running normal jobs		
	Select @NumJobsRunning = Count(JobId)
	from [dbo].[Job] j WITH (nolock)
	INNER JOIN [dbo].[JobType] jt WITH (nolock) 
	on jt.JobTypeId = j.JobType
	WHERE [j].[Status] IN (2,3)
	AND [jt].[JobTypeGroupId] = @ConstRefDataGroup

	-- Check for waiting reference data jobs
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
	INNER JOIN [dbo].[JobType] jt WITH (nolock) 
	on jt.JobTypeId = j.JobType
	WHERE [j].[Status] = 1
	AND [jt].[JobTypeGroupId] = @ConstRefDataGroup

	if @@ROWCOUNT > 0
	begin
		SET @GroupToFilter = @ConstRefDataGroup
	end
	else
	begin
		if @NumJobsRunning = 0
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

			SET @GroupToFilter = 1
			Select @NumJobsRunning = Count([Ukprn]) FROM @RunningUkprns
		End
	End

	-- Get the number of concurrent jobs we are allowed to run for the grouping type
	Select @ConcurrentExecutionCount = [ConcurrentExecutionCount]
	FROM [dbo].[JobTypeGroup] jtg
	WHERE jtg.JobTypeGroupId = @GroupToFilter

	-- Subtract the number of actual running jobs from the maximum capacity
	Set @ConcurrentExecutionCount = @ConcurrentExecutionCount - @NumJobsRunning

	-- Remove the jobs we could run, but don't have capacity for
	DELETE FROM @ReturnResults WHERE [JobId] NOT IN (SELECT TOP (@ConcurrentExecutionCount) [JobId] FROM @ReturnResults ORDER BY [Priority] DESC, JobId)

	-- Return the final list of jobs back
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
