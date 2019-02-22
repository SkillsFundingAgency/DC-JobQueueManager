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
-- Set ExtendedProperties fro DB.
	:r .\z.ExtendedProperties.sql
	
GO

RAISERROR('		   Ref Data',10,1) WITH NOWAIT;
	:r .\ReferenceData\JobStatusType.sql
	:r .\ReferenceData\JobTypeGroup.sql
	:r .\ReferenceData\JobType.sql
	:r .\ReferenceData\JobEmailTemplate.sql
	:r .\ReferenceData\CollectionType.sql
	:r .\ReferenceData\Collections.sql
	:r .\ReferenceData\ReturnPeriod_ESF.sql
	:r .\ReferenceData\ReturnPeriod_EAS1819.sql
	--:r .\ReferenceData\ReturnPeriod_EAS1920.sql
	:r .\ReferenceData\ReturnPeriod_ILR1819.sql
	--:r .\ReferenceData\ReturnPeriod_ILR1920.sql
	:r .\ReferenceData\JobTopicSubscription.sql
	:r .\ReferenceData\JobSubscriptionTask.sql
	:r .\ReferenceData\ReturnPeriod_NCS1819.sql

RAISERROR('		   Update User Account Passwords',10,1) WITH NOWAIT;
GO

RAISERROR('		         JobManagementApiUser',10,1) WITH NOWAIT;
ALTER USER [JobManagementApiUser] WITH PASSWORD = N'$(JobManagementApiUserPwd)';
GO

RAISERROR('		         JobManagementSchedulerUser',10,1) WITH NOWAIT;
ALTER USER [JobManagementSchedulerUser] WITH PASSWORD = N'$(JobManagementSchedulerUserPwd)';
GO

RAISERROR('		         JobManagementSchedulerUser',10,1) WITH NOWAIT;
ALTER USER [JobManagement_RO_User] WITH PASSWORD = N'$(ROUserPassword)';
GO

RAISERROR('		         User DSCI',10,1) WITH NOWAIT;
ALTER USER [User_DSCI] WITH PASSWORD = N'$(DsciUserPassword)';
GO

DROP PROCEDURE IF EXISTS [dbo].[usp_Add_OrganisationToCollections];
GO

RAISERROR('Completed',10,1) WITH NOWAIT;
GO
