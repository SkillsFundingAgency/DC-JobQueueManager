CREATE VIEW [dbo].[vw_CurrentCollectionReturnPeriods]
AS 
SELECT 
	   O.[Ukprn] AS UKPRN
	  ,O.[Name] AS OrgName
	  ,C.[CollectionId] AS CollectionId
	  ,C.[Name] AS CollectionName
	  ,C.[IsOpen] AS IsOpen
	  ,CT.[Type] AS CollectionType
	  ,CT.[Description] AS Description
      ,RP.[StartDateTimeUTC] AS StartDateTimeUTC
      ,RP.[EndDateTimeUTC] AS EndDateTimeUTC
      ,RP.[PeriodNumber] AS PeriodNumber
      ,RP.[CalendarMonth] AS CalendarMonth
      ,RP.[CalendarYear] AS CalendarYear
FROM [dbo].[Organisation] O
INNER JOIN [dbo].[OrganisationCollection] OC
	ON OC.[OrganisationId] = O.[OrganisationId]
INNER JOIN [dbo].[Collection] C
	ON C.[CollectionId] = OC.[CollectionId]
INNER JOIN [dbo].[CollectionType] CT
	ON C.[CollectionTypeId] = CT.[CollectionTypeId]
INNER JOIN [dbo].[ReturnPeriod] RP
	ON RP.[CollectionId] = C.[CollectionId]
   AND GETUTCDATE() BETWEEN RP.[StartDateTimeUTC] AND RP.[EndDateTimeUTC]
