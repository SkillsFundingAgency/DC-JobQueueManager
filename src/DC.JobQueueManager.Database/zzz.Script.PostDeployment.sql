/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
SET NOCOUNT ON; 

GO
RAISERROR('		   Extended Property',10,1) WITH NOWAIT;
GO

RAISERROR('		         %s - %s',10,1,'BuildNumber','$(BUILD_BUILDNUMBER)') WITH NOWAIT;
IF NOT EXISTS (SELECT name, value FROM fn_listextendedproperty('BuildNumber', default, default, default, default, default, default))
	EXEC sp_addextendedproperty @name = N'BuildNumber', @value = '$(BUILD_BUILDNUMBER)';  
ELSE
	EXEC sp_updateextendedproperty @name = N'BuildNumber', @value = '$(BUILD_BUILDNUMBER)';  
	
GO
RAISERROR('		         %s - %s',10,1,'BuildBranch','$(BUILD_BRANCHNAME)') WITH NOWAIT;
IF NOT EXISTS (SELECT name, value FROM fn_listextendedproperty('BuildBranch', default, default, default, default, default, default))
	EXEC sp_addextendedproperty @name = N'BuildBranch', @value = '$(BUILD_BRANCHNAME)';  
ELSE
	EXEC sp_updateextendedproperty @name = N'BuildBranch', @value = '$(BUILD_BRANCHNAME)';  

GO
DECLARE @DeploymentTime VARCHAR(35) = CONVERT(VARCHAR(35),GETUTCDATE(),113);
RAISERROR('		         %s - %s',10,1,'DeploymentDatetime',@DeploymentTime) WITH NOWAIT;
IF NOT EXISTS (SELECT name, value FROM fn_listextendedproperty('DeploymentDatetime', default, default, default, default, default, default))
	EXEC sp_addextendedproperty @name = N'DeploymentDatetime', @value = @DeploymentTime;  
ELSE
	EXEC sp_updateextendedproperty @name = N'DeploymentDatetime', @value = @DeploymentTime;  
GO

RAISERROR('		         %s - %s',10,1,'ReleaseName','$(RELEASE_RELEASENAME)') WITH NOWAIT;
IF NOT EXISTS (SELECT name, value FROM fn_listextendedproperty('ReleaseName', default, default, default, default, default, default))
	EXEC sp_addextendedproperty @name = N'ReleaseName', @value = '$(RELEASE_RELEASENAME)';  
ELSE
	EXEC sp_updateextendedproperty @name = N'ReleaseName', @value = '$(RELEASE_RELEASENAME)';  
GO


IF EXISTS (SELECT * FROM [sys].[objects] WHERE [type] = 'V' AND Name = 'DisplayDeploymentProperties_VW')
BEGIN 
	DROP VIEW [dbo].[DisplayDeploymentProperties_VW];
END

GO
EXEC ('CREATE VIEW [dbo].[DisplayDeploymentProperties_VW]
AS
	SELECT name, value 
	FROM fn_listextendedproperty(default, default, default, default, default, default, default);  
	');

GO

RAISERROR('		   Ref Data',10,1) WITH NOWAIT;
	:r .\ReferenceData\JobStatusType.sql
	:r .\ReferenceData\JobType.sql
	:r .\ReferenceData\JobEmailTemplate.sql
	:r .\ReferenceData\CollectionType.sql
	:r .\ReferenceData\Collections.sql
	:r .\ReferenceData\ReturnPeriod.sql

RAISERROR('		   Update User Account Passwords',10,1) WITH NOWAIT;
GO
--RAISERROR('		         JobQueueManagerApiUser',10,1) WITH NOWAIT;
--ALTER USER [JobQueueManagerApiUser] WITH PASSWORD = N'$(JobQueueManagerApiUserPwd)';
GO

--RAISERROR('		         JobQueueManagerSchedulerUser',10,1) WITH NOWAIT;
--ALTER USER [JobQueueManagerSchedulerUser] WITH PASSWORD = N'$(JobQueueManagerSchedulerUserPwd)';
GO

RAISERROR('		         JobManagementApiUser',10,1) WITH NOWAIT;
ALTER USER [JobManagementApiUser] WITH PASSWORD = N'$(JobManagementApiUserPwd)';

GO

RAISERROR('		         JobManagementSchedulerUser',10,1) WITH NOWAIT;
ALTER USER [JobManagementSchedulerUser] WITH PASSWORD = N'$(JobManagementSchedulerUserPwd)';
GO

RAISERROR('Completed',10,1) WITH NOWAIT;
GO
