CREATE USER [JobManagementSchedulerUser]
    WITH PASSWORD = N'$(JobManagementSchedulerUserPwd)';
GO
	GRANT CONNECT TO [JobManagementSchedulerUser]
GO
