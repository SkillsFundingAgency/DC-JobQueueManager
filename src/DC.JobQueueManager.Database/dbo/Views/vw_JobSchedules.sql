CREATE VIEW [dbo].[vw_JobSchedules]
AS 
SELECT 
	 S.[JobTypeId]
	,JT.[Title]
	,S.[Enabled]
	,S.[MinuteIsCadence]
	,S.[Minute]
	,S.[Hour]
	,S.[DayOfTheMonth]
	,S.[Month]
	,S.[DayOfTheWeek]
	,S.[ExecuteOnceOnly]
	,S.[LastExecuteDateTime]
FROM [dbo].[Schedule] S
INNER JOIN [dbo].[JobType] JT 
	ON JT.[JobTypeId] = S.[JobTypeId]
