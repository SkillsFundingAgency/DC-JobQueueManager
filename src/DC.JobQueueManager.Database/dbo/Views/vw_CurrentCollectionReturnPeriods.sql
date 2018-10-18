CREATE VIEW [dbo].[vw_CurrentCollectionReturnPeriods]
AS 
	SELECT 
	   O.[Ukprn] as UKPRN
	  ,O.[Name] as OrgName
	  ,C.[CollectionId] as CollectionId
	  ,C.[Name] as CollectionName
	  ,C.[IsOpen] as IsOpen
	  ,CT.[Type] as CollectionType
	  ,CT.[Description] as Description
      ,RP.[StartDateTimeUTC] as StartDateTimeUTC
      ,RP.[EndDateTimeUTC] as EndDateTimeUTC
      ,RP.[PeriodNumber] as PeriodNumber
      ,RP.[CalendarMonth] as CalendarMonth
      ,RP.[CalendarYear] as CalendarYear
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
