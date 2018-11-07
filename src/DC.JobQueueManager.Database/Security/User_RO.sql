
CREATE USER [JobManagement_RO_User] WITH PASSWORD = N'$(ROUserPassword)';
GO
  GRANT CONNECT TO [JobManagement_RO_User]
GO
