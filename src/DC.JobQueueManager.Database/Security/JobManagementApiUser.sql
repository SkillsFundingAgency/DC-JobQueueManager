CREATE USER [JobManagementApiUser]
    WITH PASSWORD = N'$(JobManagementApiUserPwd)';
GO
	GRANT CONNECT TO [JobManagementApiUser]
GO
