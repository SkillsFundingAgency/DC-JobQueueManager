
	  SELECT 1 AS UKPRN, '' AS OrgName, '' as OrgEmail, 'ILR1819' as CollectionName
UNION SELECT 2 AS UKPRN, '' AS OrgName, '' as OrgEmail, 'ILR1819' as CollectionName


SET NOCOUNT ON;
--DECLARE @CollectionNameDesc VARCHAR(250) ='ILR1819'  -- SELECT [Name] FROM [dbo].[Collection]

DECLARE @Providers TABLE (  UKPRN BIGINT,
							OrgName VARCHAR(250),
							OrgEmail VARCHAR(250), 
							CollectionName VARCHAR(250)
							PRIMARY KEY (UKPRN, CollectionName)
						 )

INSERT INTO @Providers( [UKPRN],[OrgName],[OrgEmail],[CollectionName])
SELECT DISTINCT [UKPRN],[OrgName],[OrgEmail],[CollectionName]
FROM
(
-------------------------------------------------------- PUT DATA - START-------------------------------------------------------- 
	  SELECT 1 AS UKPRN, '' AS OrgName, '' as OrgEmail, 'ILR1819' as CollectionName
UNION SELECT 2 AS UKPRN, '' AS OrgName, '' as OrgEmail, 'ILR1819' as CollectionName

-------------------------------------------------------- PUT DATA - END  -------------------------------------------------------- 
) as NewRecords

DECLARE @UKPRN BIGINT ,@OrgName VARCHAR(250),@OrgEmail VARCHAR(250), @CollectionName VARCHAR(250)

DECLARE Providers_Cursor CURSOR READ_ONLY FOR SELECT * FROM @Providers 

OPEN Providers_Cursor;  
FETCH NEXT FROM Providers_Cursor INTO @UKPRN,@OrgName,@OrgEmail, @CollectionName;  
WHILE @@FETCH_STATUS = 0  
   BEGIN  
	SELECT  @UKPRN as UKPRN ,@OrgName as OrgName,@OrgEmail as OrgEmail, @CollectionName as CollectionName

	EXEC [dbo].[usp_Add_UKPRN_to_Collection]
			@CollectionName = @CollectionName
		   ,@UKPRN = @UKPRN
		   ,@OrgName = @OrgName
		   ,@OrgEmail = @OrgEmail

      FETCH NEXT FROM Providers_Cursor INTO @UKPRN,@OrgName,@OrgEmail, @CollectionName; 
   END;  
CLOSE Providers_Cursor;  
DEALLOCATE Providers_Cursor;  
GO  

SELECT * FROM [dbo].[vw_CurrentCollectionReturnPeriods]
GO




/*

FROM [CollectionsManagement] DB ONLY


SELECT ' UNION SELECT ' + CONVERT(VARCHAR,O.UKPRN) + ' AS UKPRN, ''' + o.[Name]  + ''' AS OrgName, ''' + o.[Email] + ''' as OrgEmail, ''' + C.[Name] + ''' as CollectionName'
FROM [dbo].[OrganisationCollection] OC
INNER JOIN [dbo].[Organisation] O
  ON O.OrganisationId = OC.OrganisationId
INNER JOIN [dbo].[Collection] C
  ON c.CollectionId = oc.CollectionId
INNER JOIN [dbo].[CollectionType] CT
  ON CT.CollectionTypeId = C.CollectionTypeId
--WHERE C.[Name] = 'ILR1819'

*/

GO