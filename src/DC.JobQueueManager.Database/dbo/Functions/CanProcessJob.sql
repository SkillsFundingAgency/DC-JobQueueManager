-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
--DROP FUNCTION IsReturnWindowOpen
CREATE FUNCTION CanProcessJob
(
	@collectionId int,
	@dateTimeSubmittedUTC datetime,
	@jobType smallint,
	@isFirstStage bit 
)
RETURNS int
AS
BEGIN

	DECLARE @overrideFlag bit = null

	--Get the override flag for jobType
	Select @overrideFlag = ProcessingOverrideFlag from JobType Where JobTypeId = @jobType
	
	--if override flag on then continue processing
	if @overrideFlag = 1  
		Return 1

	--if override flag 0 then do not process anything
	if @overrideFlag = 0
		Return 0
	
	--If reference data type then don't check collections etc 
	if @jobType = 4
		Return 1

	--If ILR type then allow first stage 
	if @jobType = 1 AND IsNull(@isFirstStage,1) = 1
		Return 1

	--if period open and no override set its ok
	 If Exists (select 1 from ReturnPeriod rp Where CollectionId = @collectionId And
				@dateTimeSubmittedUTC between rp.StartDateTimeUTC AND rp.EndDateTimeUTC)
		Return 1

	Return 0
END