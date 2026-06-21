using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using Newtonsoft.Json.Linq;

namespace Server.Core;

public class PublicController
{
    [ResourceMethod("dashboard/:token")]
    public ValueTask<object> GetPublicDashboard(string token)
    {
        var row = DatabasePersistence.LoadEntityByShareToken("Dashboards", token);
        if (row == null)
            throw new ProviderException(ResponseStatus.NotFound, "Dashboard not found or not shared");

        var isPublic = (row["ispublic"] ?? row["IsPublic"])?.Value<bool>() ?? false;
        if (!isPublic)
            throw new ProviderException(ResponseStatus.NotFound, "Dashboard not found or not shared");

        var configJson = row["config"]?.ToString() ?? row["Config"]?.ToString();
        object config = null;
        if (!string.IsNullOrEmpty(configJson))
        {
            try { config = Newtonsoft.Json.JsonConvert.DeserializeObject(configJson); }
            catch { config = configJson; }
        }

        return ValueTask.FromResult<object>(new
        {
            id = row["id"]?.ToString() ?? row["Id"]?.ToString(),
            name = row["name"]?.ToString() ?? row["Name"]?.ToString(),
            shareToken = token,
            config
        });
    }

    [ResourceMethod("report/:token")]
    public ValueTask<object> GetPublicReport(string token)
    {
        var row = DatabasePersistence.LoadEntityByShareToken("Reports", token);
        if (row == null)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found or not shared");

        var isPublic = (row["ispublic"] ?? row["IsPublic"])?.Value<bool>() ?? false;
        if (!isPublic)
            throw new ProviderException(ResponseStatus.NotFound, "Report not found or not shared");

        var configJson = row["config"]?.ToString() ?? row["Config"]?.ToString();
        object config = null;
        if (!string.IsNullOrEmpty(configJson))
        {
            try { config = Newtonsoft.Json.JsonConvert.DeserializeObject(configJson); }
            catch { config = configJson; }
        }

        return ValueTask.FromResult<object>(new
        {
            id = row["id"]?.ToString() ?? row["Id"]?.ToString(),
            name = row["name"]?.ToString() ?? row["Name"]?.ToString(),
            shareToken = token,
            config
        });
    }
}
