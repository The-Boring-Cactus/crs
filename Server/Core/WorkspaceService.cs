using Server.Core.Models;
using System.Text.Json;

namespace Server.Core;

public interface IWorkspaceService
{
    Task<WorkspaceData?> GetWorkspaceAsync(string id, string userId);
    Task<List<WorkspaceData>> ListWorkspacesAsync(string userId, string? workspaceType = null);
    Task<WorkspaceData> SaveWorkspaceAsync(WorkspaceData workspace);
    Task<bool> DeleteWorkspaceAsync(string id, string userId);
}

public class WorkspaceService : IWorkspaceService
{
    private readonly IReportsCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public WorkspaceService(IReportsCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<WorkspaceData?> GetWorkspaceAsync(string id, string userId)
    {
        var cacheKey = $"workspace:{userId}:{id}";

        try
        {
            return await _cache.GetOrExecuteAsync(
                cacheKey,
                async () => (WorkspaceData?)null,
                TimeSpan.FromDays(365)
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<WorkspaceData>> ListWorkspacesAsync(string userId, string? workspaceType = null)
    {
        var cacheKey = workspaceType != null
            ? $"workspaces:{userId}:{workspaceType}"
            : $"workspaces:{userId}:all";

        try
        {
            return await _cache.GetOrExecuteAsync(
                cacheKey,
                async () => new List<WorkspaceData>(),
                TimeSpan.FromMinutes(5)
            );
        }
        catch
        {
            return new List<WorkspaceData>();
        }
    }

    public async Task<WorkspaceData> SaveWorkspaceAsync(WorkspaceData workspace)
    {
        workspace.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(workspace.Id))
        {
            workspace.Id = Guid.NewGuid().ToString();
            workspace.CreatedAt = DateTime.UtcNow;
        }

        // Save individual workspace
        var workspaceCacheKey = $"workspace:{workspace.UserId}:{workspace.Id}";
        await _cache.GetOrExecuteAsync(
            workspaceCacheKey,
            async () => workspace,
            TimeSpan.FromDays(365)
        );

        // Update the list of workspaces
        var listCacheKey = $"workspaces:{workspace.UserId}:all";
        var allWorkspaces = await ListWorkspacesAsync(workspace.UserId);

        var existingIndex = allWorkspaces.FindIndex(w => w.Id == workspace.Id);
        if (existingIndex >= 0)
        {
            allWorkspaces[existingIndex] = workspace;
        }
        else
        {
            allWorkspaces.Add(workspace);
        }

        // Save updated list
        await _cache.GetOrExecuteAsync(
            listCacheKey,
            async () => allWorkspaces,
            TimeSpan.FromMinutes(5)
        );

        // Update type-specific list
        var typedListCacheKey = $"workspaces:{workspace.UserId}:{workspace.WorkspaceType}";
        _cache.InvalidateQuery(typedListCacheKey);

        return workspace;
    }

    public async Task<bool> DeleteWorkspaceAsync(string id, string userId)
    {
        var workspaceCacheKey = $"workspace:{userId}:{id}";
        _cache.InvalidateQuery(workspaceCacheKey);

        // Update the lists
        var listCacheKey = $"workspaces:{userId}:all";
        var allWorkspaces = await ListWorkspacesAsync(userId);
        var workspace = allWorkspaces.FirstOrDefault(w => w.Id == id);

        if (workspace != null)
        {
            allWorkspaces.RemoveAll(w => w.Id == id);

            // Re-save the updated list
            await _cache.GetOrExecuteAsync(
                listCacheKey,
                async () => allWorkspaces,
                TimeSpan.FromMinutes(5)
            );

            // Invalidate type-specific list
            var typedListCacheKey = $"workspaces:{userId}:{workspace.WorkspaceType}";
            _cache.InvalidateQuery(typedListCacheKey);

            return true;
        }

        return false;
    }
}
