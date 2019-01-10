
CREATE VIEW [dbo].[vw_OrganisationCollectionAssignment]
AS 
	SELECT TOP 100 PERCENT
	         O.UKPRN, O.[Name]
			,(CASE WHEN ILR1819.[CollectionName] IS NULL THEN 'NO' ELSE 'YES' END) AS ILR1819
			,(CASE WHEN ESF.[CollectionName] IS NULL THEN 'NO' ELSE 'YES' END) AS ESF
			,(CASE WHEN EAS1819.[CollectionName] IS NULL THEN 'NO' ELSE 'YES' END) AS EAS1819
	FROM [dbo].[Organisation] O
	LEFT JOIN [dbo].[vw_CurrentCollectionReturnPeriods] ILR1819
	  ON O.[UKPRN] = ILR1819.[UKPRN]
	 AND  ILR1819.[CollectionName] = 'ILR1819' 
		LEFT JOIN [dbo].[vw_CurrentCollectionReturnPeriods] ESF
	  ON O.[UKPRN] = ESF.[UKPRN]
	 AND  ESF.[CollectionName] = 'ESF' 
	LEFT JOIN [dbo].[vw_CurrentCollectionReturnPeriods] EAS1819
	  ON O.[UKPRN] = EAS1819.[UKPRN]
	 AND  EAS1819.[CollectionName] = 'EAS1819' 
	 ORDER BY O.UKPRN

