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
    public string Type { get; set; } // mssql, postgres, mysql
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
            "mysql" => $"Server={Host};Port={Port};Database={DatabaseName};Uid={Username};Pwd={Password};",
            _ => throw new InvalidOperationException($"Unsupported database type: {Type}")
        };
    }

    public string GetCreateTablesSQL()
    {
        return Type?.ToLower() switch
        {
            "mssql" => GetMssqlCreateTablesSQL(),
            "postgres" => GetPostgresCreateTablesSQL(),
            "mysql" => GetMysqlCreateTablesSQL(),
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
