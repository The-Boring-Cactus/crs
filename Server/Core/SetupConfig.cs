using Newtonsoft.Json;

namespace Server.Core;

public class SetupConfig
{
    public DatabaseConfig Database { get; set; }
    public AdminUserConfig AdminUser { get; set; }
    public SmtpConfig Smtp { get; set; }
    public bool IsConfigured { get; set; } = false;
    public DateTime ConfiguredAt { get; set; }

    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public static SetupConfig Load()
    {
        if (!File.Exists(ConfigPath))
            return new SetupConfig();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonConvert.DeserializeObject<SetupConfig>(json) ?? new SetupConfig();
        }
        catch
        {
            return new SetupConfig();
        }
    }

    public void Save()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(ConfigPath, json);
    }
}

public class DatabaseConfig
{
    public string Type { get; set; } // mssql, postgres, mysql, oracle
    public string Host { get; set; }
    public int Port { get; set; }
    public string DatabaseName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public string GetConnectionString()
    {
        return Type?.ToLower() switch
        {
            "mssql" => $"Server={Host},{Port};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;",
            "postgres" => $"Host={Host};Port={Port};Database={DatabaseName};Username={Username};Password={Password};",
            "postgresql" => $"Host={Host};Port={Port};Database={DatabaseName};Username={Username};Password={Password};",
            "mysql" => $"Server={Host};Port={Port};Database={DatabaseName};Uid={Username};Pwd={Password};",
            "oracle" => $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port}))(CONNECT_DATA=(SERVICE_NAME={DatabaseName})));User Id={Username};Password={Password};",
            _ => throw new InvalidOperationException($"Unsupported database type: {Type}")
        };
    }

    public string GetCreateTablesSQL()
    {
        return Type?.ToLower() switch
        {
            "mssql" => GetMssqlCreateTablesSQL(),
            "postgres" => GetPostgresCreateTablesSQL(),
            "postgresql" => GetPostgresCreateTablesSQL(),
            "mysql" => GetMysqlCreateTablesSQL(),
            "oracle" => GetOracleCreateTablesSQL(),
            _ => throw new InvalidOperationException($"Unsupported database type: {Type}")
        };
    }

    private string GetMssqlCreateTablesSQL() => @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Salt NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    Roles NVARCHAR(500) DEFAULT 'admin'
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Dashboards' AND xtype='U')
CREATE TABLE Dashboards (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Config NVARCHAR(MAX),
    IsPublic BIT DEFAULT 0,
    ShareToken NVARCHAR(100),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reports' AND xtype='U')
CREATE TABLE Reports (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Config NVARCHAR(MAX),
    IsPublic BIT DEFAULT 0,
    ShareToken NVARCHAR(100),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DatabaseConnections' AND xtype='U')
CREATE TABLE DatabaseConnections (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Host NVARCHAR(200),
    Port INT,
    DatabaseName NVARCHAR(200),
    Username NVARCHAR(200),
    Password NVARCHAR(500),
    ConnectionString NVARCHAR(MAX),
    IsGlobal BIT DEFAULT 0,
    SharedWith NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Datasets' AND xtype='U')
CREATE TABLE Datasets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Config NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SqlScripts' AND xtype='U')
CREATE TABLE SqlScripts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Language NVARCHAR(50) DEFAULT 'sql',
    Code NVARCHAR(MAX),
    DatabaseConnectionId NVARCHAR(200),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CodeScripts' AND xtype='U')
CREATE TABLE CodeScripts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Language NVARCHAR(50) DEFAULT 'csharp',
    Code NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
";

    private string GetPostgresCreateTablesSQL() => @"
CREATE TABLE IF NOT EXISTS Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(100) NOT NULL UNIQUE,
    FullName VARCHAR(200) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    Salt VARCHAR(500) NOT NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    IsActive BOOLEAN DEFAULT TRUE,
    Roles VARCHAR(500) DEFAULT 'admin'
);

CREATE TABLE IF NOT EXISTS Dashboards (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Config TEXT,
    IsPublic BOOLEAN DEFAULT FALSE,
    ShareToken VARCHAR(100),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS Reports (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Config TEXT,
    IsPublic BOOLEAN DEFAULT FALSE,
    ShareToken VARCHAR(100),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS DatabaseConnections (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Host VARCHAR(200),
    Port INT,
    DatabaseName VARCHAR(200),
    Username VARCHAR(200),
    Password VARCHAR(500),
    ConnectionString TEXT,
    IsGlobal BOOLEAN DEFAULT FALSE,
    SharedWith TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS Datasets (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Config TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS SqlScripts (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Language VARCHAR(50) DEFAULT 'sql',
    Code TEXT,
    DatabaseConnectionId VARCHAR(200),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS CodeScripts (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Language VARCHAR(50) DEFAULT 'csharp',
    Code TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);
";

    private string GetMysqlCreateTablesSQL() => @"
CREATE TABLE IF NOT EXISTS Users (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    Username VARCHAR(100) NOT NULL UNIQUE,
    FullName VARCHAR(200) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    Salt VARCHAR(500) NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    Roles VARCHAR(500) DEFAULT 'admin'
);

CREATE TABLE IF NOT EXISTS Dashboards (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Config LONGTEXT,
    IsPublic BOOLEAN DEFAULT FALSE,
    ShareToken VARCHAR(100),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS Reports (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Config LONGTEXT,
    IsPublic BOOLEAN DEFAULT FALSE,
    ShareToken VARCHAR(100),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS DatabaseConnections (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Host VARCHAR(200),
    Port INT,
    DatabaseName VARCHAR(200),
    Username VARCHAR(200),
    Password VARCHAR(500),
    ConnectionString TEXT,
    IsGlobal BOOLEAN DEFAULT FALSE,
    SharedWith TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS Datasets (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Config LONGTEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS SqlScripts (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Language VARCHAR(50) DEFAULT 'sql',
    Code LONGTEXT,
    DatabaseConnectionId VARCHAR(200),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS CodeScripts (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Language VARCHAR(50) DEFAULT 'csharp',
    Code LONGTEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
";

    public string GetMigrationSQL()
    {
        return Type?.ToLower() switch
        {
            "mssql" => GetMssqlMigrationSQL(),
            "postgres" => GetPostgresMigrationSQL(),
            "postgresql" => GetPostgresMigrationSQL(),
            "mysql" => GetMysqlMigrationSQL(),
            "oracle" => GetOracleMigrationSQL(),
            _ => ""
        };
    }

    private string GetMssqlMigrationSQL() => @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Projects' AND xtype='U')
CREATE TABLE Projects (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Dashboards') AND name = 'ProjectId')
    ALTER TABLE Dashboards ADD ProjectId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SqlScripts') AND name = 'ProjectId')
    ALTER TABLE SqlScripts ADD ProjectId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CodeScripts') AND name = 'ProjectId')
    ALTER TABLE CodeScripts ADD ProjectId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Datasets') AND name = 'ProjectId')
    ALTER TABLE Datasets ADD ProjectId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DatabaseConnections') AND name = 'ProjectId')
    ALTER TABLE DatabaseConnections ADD ProjectId UNIQUEIDENTIFIER NULL;
";

    private string GetPostgresMigrationSQL() => @"
CREATE TABLE IF NOT EXISTS Projects (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);
ALTER TABLE Dashboards ADD COLUMN IF NOT EXISTS ProjectId UUID;
ALTER TABLE SqlScripts ADD COLUMN IF NOT EXISTS ProjectId UUID;
ALTER TABLE CodeScripts ADD COLUMN IF NOT EXISTS ProjectId UUID;
ALTER TABLE Datasets ADD COLUMN IF NOT EXISTS ProjectId UUID;
ALTER TABLE DatabaseConnections ADD COLUMN IF NOT EXISTS ProjectId UUID;
";

    private string GetMysqlMigrationSQL() => @"
CREATE TABLE IF NOT EXISTS Projects (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    UserId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Description LONGTEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
ALTER TABLE Dashboards ADD COLUMN IF NOT EXISTS ProjectId CHAR(36) NULL;
ALTER TABLE SqlScripts ADD COLUMN IF NOT EXISTS ProjectId CHAR(36) NULL;
ALTER TABLE CodeScripts ADD COLUMN IF NOT EXISTS ProjectId CHAR(36) NULL;
ALTER TABLE Datasets ADD COLUMN IF NOT EXISTS ProjectId CHAR(36) NULL;
ALTER TABLE DatabaseConnections ADD COLUMN IF NOT EXISTS ProjectId CHAR(36) NULL;
";

    private string GetOracleMigrationSQL() => @"
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Projects (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Description CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE Dashboards ADD ProjectId CHAR(36)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SqlScripts ADD ProjectId CHAR(36)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE CodeScripts ADD ProjectId CHAR(36)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE Datasets ADD ProjectId CHAR(36)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE DatabaseConnections ADD ProjectId CHAR(36)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/
";

    private string GetOracleCreateTablesSQL() => @"
BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Users (
    Id CHAR(36) PRIMARY KEY,
    Username VARCHAR2(100) NOT NULL UNIQUE,
    FullName VARCHAR2(200) NOT NULL,
    Email VARCHAR2(200) NOT NULL,
    PasswordHash VARCHAR2(500) NOT NULL,
    Salt VARCHAR2(500) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    IsActive NUMBER(1) DEFAULT 1,
    Roles VARCHAR2(500) DEFAULT ''admin''
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Dashboards (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Config CLOB,
    IsPublic NUMBER(1) DEFAULT 0,
    ShareToken VARCHAR2(100),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Reports (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Config CLOB,
    IsPublic NUMBER(1) DEFAULT 0,
    ShareToken VARCHAR2(100),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE DatabaseConnections (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Type VARCHAR2(50) NOT NULL,
    Host VARCHAR2(200),
    Port NUMBER(5),
    DatabaseName VARCHAR2(200),
    Username VARCHAR2(200),
    Password VARCHAR2(500),
    ConnectionString CLOB,
    IsGlobal NUMBER(1) DEFAULT 0,
    SharedWith CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE Datasets (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Config CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE SqlScripts (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Language VARCHAR2(50) DEFAULT ''sql'',
    Code CLOB,
    DatabaseConnectionId VARCHAR2(200),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE TABLE CodeScripts (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL REFERENCES Users(Id),
    Name VARCHAR2(200) NOT NULL,
    Language VARCHAR2(50) DEFAULT ''csharp'',
    Code CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
  )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/
";
}

public class AdminUserConfig
{
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    // Password is NOT stored in config - only stored hashed in DB
}

public class SmtpConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string FromAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool UseSsl { get; set; }
    public bool IsConfigured { get; set; } = false;
}
