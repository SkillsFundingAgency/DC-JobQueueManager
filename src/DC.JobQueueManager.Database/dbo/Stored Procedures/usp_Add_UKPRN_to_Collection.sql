CREATE PROCEDURE [dbo].[usp_Add_UKPRN_to_Collection]
(    @CollectionName VARCHAR(250)
    ,@UKPRN BIGINT
	,@OrgName VARCHAR(250)
    ,@OrgEmail VARCHAR(250)
)	   
AS
	BEGIN
	SET NOCOUNT ON;

	IF (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionName) IS NULL
	BEGIN
	
		RAISERROR('Unable to Find Collection : %s',10,1,@CollectionName) WITH NOWAIT;
		RETURN;
	END

	SET @OrgName = LTRIM(RTRIM(@OrgName));
	SET @OrgEmail = LTRIM(RTRIM(@OrgEmail));

	IF NOT EXISTS (SELECT * FROM [dbo].[Organisation] WHERE [Ukprn] = @UKPRN)
	BEGIN
		DECLARE @OrgId INT = (SELECT ISNULL(MAX([OrganisationId]),0)+1 FROM [dbo].[Organisation]);

		INSERT [dbo].[Organisation] ([OrganisationId], [OrgId], [Ukprn], [Name], [Email])
		VALUES (@OrgId, @OrgId, @UKPRN, @OrgName, @OrgEmail);
		RAISERROR('Added UKPRN : %I64d | Org : %s',10,1,@UKPRN,@OrgName ) WITH NOWAIT;
	END

	IF NOT EXISTS (
					SELECT 
						   O.[Ukprn] as UKPRN
						  ,C.[CollectionId] as CollectionId
						  ,C.[Name] as CollectionName
						  ,C.[IsOpen] as IsOpen
					FROM [dbo].[Organisation] O
					INNER JOIN [dbo].[OrganisationCollection] OC
						ON OC.[OrganisationId] = O.[OrganisationId]
					INNER JOIN [dbo].[Collection] C
						ON C.[CollectionId] = OC.[CollectionId]
					WHERE O.[Ukprn] = @UKPRN
					  AND C.[Name] = @CollectionName
	)
	 BEGIN
		DECLARE @OrganisationId INT = (SELECT [OrganisationId] FROM [dbo].[Organisation] WHERE [Ukprn] = @UKPRN);
		DECLARE @CollectionId INT = (SELECT [CollectionId] FROM [dbo].[Collection] WHERE [Name] = @CollectionName);

		--SELECT @OrganisationId as OrganisationId, @CollectionId as CollectionId;

		IF ((@OrganisationId IS NULL )OR(@CollectionId IS NULL)) 
		  BEGIN
			IF (@OrganisationId IS NULL )
			BEGIN
				RAISERROR('Error Getting Org IDs for UKPRN : %I64d',17,1,@UKPRN) WITH NOWAIT;
				--SELECT * FROM [dbo].[Organisation] WHERE [Ukprn] = @UKPRN
			END		
		  END
		ELSE
		  BEGIN
			INSERT [dbo].[OrganisationCollection] ([OrganisationId], [CollectionId]) 
			SELECT @OrganisationId, @CollectionId
			RAISERROR('Added Collection : %s to UKPRN : %I64d - %s',10,1,@CollectionName,@UKPRN,@OrgName ) WITH NOWAIT;
		  END
	 END
	ELSE
	 BEGIN
		RAISERROR('UKPRN : %I64d  - Already has access to Collection : %s',10,1,@UKPRN,@CollectionName) WITH NOWAIT;
	 END	
END	 
GO