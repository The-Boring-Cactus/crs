namespace Server.Core.Models;

public class WorkspaceData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string WorkspaceType { get; set; } = string.Empty; // "CsEditor", "SqlEditor", "Dashboard", "Excel", "Database"
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object Content { get; set; } = new { };
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class CsEditorContent
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
}

public class SqlEditorContent
{
    public string Query { get; set; } = string.Empty;
    public string DatabaseId { get; set; } = string.Empty;
}

public class DashboardContent
{
    public string Title { get; set; } = string.Empty;
    public List<DashboardComponent> Components { get; set; } = new();
}

public class DashboardComponent
{
    public string I { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Value { get; set; }
    public object? ChartData { get; set; }
    public object? TableData { get; set; }
    public object? Columns { get; set; }
    public bool Static { get; set; }
}

public class ExcelContent
{
    public List<ExcelSheet> Sheets { get; set; } = new();
}

public class ExcelSheet
{
    public string Name { get; set; } = string.Empty;
    public List<List<object>> Data { get; set; } = new();
}

public class DatabaseContent
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseType { get; set; } = string.Empty; // "MSSQL", "PostgreSQL", "MySQL", etc.
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Should be encrypted in production
}
