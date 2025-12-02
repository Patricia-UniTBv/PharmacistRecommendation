-- =======================================================================
-- PharmacistRecommendation - Database Setup Script
-- =======================================================================
-- This script sets up the database and SQL login for client connections
-- Run this on the SERVER computer after installing SQL Server Express
-- =======================================================================

USE [master];
GO

PRINT '========================================================================';
PRINT 'PharmacistRecommendation - Database Setup';
PRINT '========================================================================';
PRINT '';

-- =======================================================================
-- Step 1: Create Database (if it doesn't exist)
-- =======================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PharmacistRecommendationDB')
BEGIN
    PRINT 'Creating database: PharmacistRecommendationDB...';
    CREATE DATABASE [PharmacistRecommendationDB];
    PRINT 'Database created successfully!';
END
ELSE
BEGIN
    PRINT 'Database PharmacistRecommendationDB already exists.';
END
GO

PRINT '';

-- =======================================================================
-- Step 2: Create SQL Login for Client Connections
-- =======================================================================

USE [master];
GO

DECLARE @LoginExists INT;
SET @LoginExists = (SELECT COUNT(*) FROM sys.sql_logins WHERE name = 'appuser');

IF @LoginExists = 0
BEGIN
    PRINT 'Creating SQL login: appuser...';
    -- IMPORTANT: Change this password to a strong password!
    CREATE LOGIN [appuser] WITH 
        PASSWORD = 'P@ssw0rd123!Change_This_Password', 
        DEFAULT_DATABASE = [PharmacistRecommendationDB],
        CHECK_EXPIRATION = OFF,
        CHECK_POLICY = OFF;
    PRINT 'Login created successfully!';
    PRINT '';
    PRINT 'IMPORTANT: Remember to change the default password above!';
END
ELSE
BEGIN
    PRINT 'Login "appuser" already exists.';
    PRINT 'If you need to reset the password, run:';
    PRINT '  ALTER LOGIN [appuser] WITH PASSWORD = ''YourNewPassword'';';
END
GO

PRINT '';

-- =======================================================================
-- Step 3: Create Database User and Grant Permissions
-- =======================================================================

USE [PharmacistRecommendationDB];
GO

-- Create user if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'appuser')
BEGIN
    PRINT 'Creating database user: appuser...';
    CREATE USER [appuser] FOR LOGIN [appuser];
    PRINT 'User created successfully!';
END
ELSE
BEGIN
    PRINT 'User "appuser" already exists in database.';
END
GO

PRINT '';
PRINT 'Granting permissions to appuser...';

-- Grant read permissions
IF IS_ROLEMEMBER('db_datareader', 'appuser') = 0
BEGIN
    ALTER ROLE [db_datareader] ADD MEMBER [appuser];
    PRINT '  - Granted db_datareader role';
END
ELSE
BEGIN
    PRINT '  - Already has db_datareader role';
END

-- Grant write permissions
IF IS_ROLEMEMBER('db_datawriter', 'appuser') = 0
BEGIN
    ALTER ROLE [db_datawriter] ADD MEMBER [appuser];
    PRINT '  - Granted db_datawriter role';
END
ELSE
BEGIN
    PRINT '  - Already has db_datawriter role';
END

-- Grant DDL permissions (for Entity Framework migrations)
IF IS_ROLEMEMBER('db_ddladmin', 'appuser') = 0
BEGIN
    ALTER ROLE [db_ddladmin] ADD MEMBER [appuser];
    PRINT '  - Granted db_ddladmin role';
END
ELSE
BEGIN
    PRINT '  - Already has db_ddladmin role';
END

GO

PRINT '';

-- =======================================================================
-- Step 4: Verification
-- =======================================================================

PRINT '========================================================================';
PRINT 'Verification';
PRINT '========================================================================';
PRINT '';

-- Verify database exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PharmacistRecommendationDB')
BEGIN
    PRINT '[OK] Database exists: PharmacistRecommendationDB';
END
ELSE
BEGIN
    PRINT '[ERROR] Database not found!';
END

-- Verify login exists
USE [master];
IF EXISTS (SELECT name FROM sys.sql_logins WHERE name = 'appuser')
BEGIN
    PRINT '[OK] SQL login exists: appuser';
END
ELSE
BEGIN
    PRINT '[ERROR] SQL login not found!';
END

-- Verify user exists and has permissions
USE [PharmacistRecommendationDB];
GO

IF EXISTS (SELECT name FROM sys.database_principals WHERE name = 'appuser')
BEGIN
    PRINT '[OK] Database user exists: appuser';
    
    -- Check role memberships
    DECLARE @Roles NVARCHAR(MAX) = '';
    SELECT @Roles = @Roles + r.name + ', '
    FROM sys.database_role_members drm
    JOIN sys.database_principals u ON drm.member_principal_id = u.principal_id
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    WHERE u.name = 'appuser';
    
    IF LEN(@Roles) > 0
    BEGIN
        SET @Roles = LEFT(@Roles, LEN(@Roles) - 1); -- Remove trailing comma
        PRINT '[OK] User roles: ' + @Roles;
    END
END
ELSE
BEGIN
    PRINT '[ERROR] Database user not found!';
END

PRINT '';

-- =======================================================================
-- Connection String Information
-- =======================================================================

PRINT '========================================================================';
PRINT 'Connection String Information';
PRINT '========================================================================';
PRINT '';

-- Get server name and IP
DECLARE @ServerName NVARCHAR(128);
DECLARE @InstanceName NVARCHAR(128);

SET @ServerName = CAST(SERVERPROPERTY('MachineName') AS NVARCHAR(128));
SET @InstanceName = CAST(SERVERPROPERTY('InstanceName') AS NVARCHAR(128));

IF @InstanceName IS NOT NULL
    SET @ServerName = @ServerName + '\' + @InstanceName;

PRINT 'Server Instance: ' + @ServerName;
PRINT '';
PRINT 'Connection strings to use:';
PRINT '';
PRINT 'For SERVER mode (Windows Authentication):';
PRINT '  Server=' + @ServerName + ';Database=PharmacistRecommendationDB;Integrated Security=true;TrustServerCertificate=true;';
PRINT '';
PRINT 'For CLIENT mode (SQL Authentication):';
PRINT '  Server=[SERVER_IP]\' + ISNULL(@InstanceName, 'SQLEXPRESS') + ';Database=PharmacistRecommendationDB;User Id=appuser;Password=[YOUR_PASSWORD];TrustServerCertificate=true;';
PRINT '';
PRINT 'IMPORTANT: Replace [SERVER_IP] with your server''s IP address';
PRINT 'IMPORTANT: Replace [YOUR_PASSWORD] with the password you set';
PRINT '';

-- =======================================================================
-- Next Steps
-- =======================================================================

PRINT '========================================================================';
PRINT 'Next Steps';
PRINT '========================================================================';
PRINT '';
PRINT '1. Find your server''s IP address:';
PRINT '   - Open Command Prompt';
PRINT '   - Run: ipconfig';
PRINT '   - Look for IPv4 Address (e.g., 192.168.1.100)';
PRINT '';
PRINT '2. Configure SQL Server for remote connections:';
PRINT '   - Open SQL Server Configuration Manager';
PRINT '   - Enable TCP/IP protocol';
PRINT '   - Set TCP port to 1433';
PRINT '   - Restart SQL Server service';
PRINT '';
PRINT '3. Configure Windows Firewall:';
PRINT '   - Open Command Prompt as Administrator';
PRINT '   - Run: netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433';
PRINT '';
PRINT '4. Test the connection from a client computer using SQL Server Management Studio';
PRINT '';
PRINT 'For detailed instructions, see COMPLETE_DEPLOYMENT_GUIDE.md';
PRINT '';

-- =======================================================================
-- Optional: View Database Information
-- =======================================================================

PRINT '========================================================================';
PRINT 'Database Information';
PRINT '========================================================================';
PRINT '';

USE [PharmacistRecommendationDB];
GO

-- Check if tables exist (from migrations)
DECLARE @TableCount INT;
SELECT @TableCount = COUNT(*) 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
  AND TABLE_SCHEMA = 'dbo';

IF @TableCount > 0
BEGIN
    PRINT 'Database contains ' + CAST(@TableCount AS VARCHAR(10)) + ' tables.';
    PRINT '';
    PRINT 'Tables in database:';
    
    SELECT '  - ' + TABLE_NAME AS TableName
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_TYPE = 'BASE TABLE' 
      AND TABLE_SCHEMA = 'dbo'
    ORDER BY TABLE_NAME;
END
ELSE
BEGIN
    PRINT 'Database is empty. Tables will be created when you run the application for the first time.';
END

PRINT '';
PRINT '========================================================================';
PRINT 'Setup Complete!';
PRINT '========================================================================';
PRINT '';

GO
