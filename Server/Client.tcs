using FunctEngine;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Server.Helpers;

namespace Server;

public class Client
{

    public IWebsocketConnection Socket;
    private CodeEngine interpreter = new CodeEngine("");
    private WebSocketMessageClient _clientMsg;
    public string Uuid ;
    public Client (IWebsocketConnection socket, string uuid)
    {
        this.Socket = socket;
        Uuid = uuid;
        _clientMsg = new WebSocketMessageClient(this.Socket);
        _clientMsg.AuthenticationMessageReceived += AuthenticationMessage;  
        _clientMsg.CommandMessageReceived += CommandMessage;
        _clientMsg.TextMessageReceived += TextMessage;
        _clientMsg.NotificationMessageReceived += NotificationMessage;
        _clientMsg.ErrorMessageReceived += ErrorMessage;
        _clientMsg.DataMessageReceived += DataMessage;
        _clientMsg.HeartbeatMessageReceived += HeartbeatMessage;
        _clientMsg.ErrorOccurred += ErrorOccurred;
        interpreter.StatusUpdate += TestScriptOnStatusUpdate; 
    }

    private void HeartbeatMessage(object sender, MessageReceivedEventArgs e)
    {
        // Not to process
    }

    private void DataMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ErrorMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void NotificationMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void TextMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static Dictionary<string, List<JObject>> _userScripts = new Dictionary<string, List<JObject>>();
    private static Dictionary<string, List<JObject>> _userDatabaseConnections = new Dictionary<string, List<JObject>>();
    private static Dictionary<string, List<JObject>> _userDatasets = new Dictionary<string, List<JObject>>();
    private static Dictionary<string, List<JObject>> _userExcels = new Dictionary<string, List<JObject>>();
    private static Dictionary<string, List<JObject>> _userDashboards = new Dictionary<string, List<JObject>>();

    private void CommandMessage(object sender, MessageReceivedEventArgs e)
    {
        var cmdMessage = e.Message as CommandMessage;
        if (cmdMessage == null) return;

        var parameters = cmdMessage.Parameters;
        
        ResponseMessage response = new ResponseMessage
        {
            RequestId = cmdMessage.Id,
            Status = MessageStatus.Success,
            ErrorMessage = ""
        };

        if (!_userScripts.ContainsKey(Uuid))
        {
            _userScripts[Uuid] = new List<JObject>();
        }
        
        if (!_userDatabaseConnections.ContainsKey(Uuid))
        {
            _userDatabaseConnections[Uuid] = new List<JObject>();
        }

        if (!_userDatasets.ContainsKey(Uuid))
        {
            _userDatasets[Uuid] = new List<JObject>();
        }

        if (!_userExcels.ContainsKey(Uuid))
        {
            _userExcels[Uuid] = new List<JObject>();
        }

        if (!_userDashboards.ContainsKey(Uuid))
        {
            _userDashboards[Uuid] = new List<JObject>();
        }

        try 
        {
            switch (cmdMessage.Command)
            {
                case "ExecuteCs":
                    if (parameters.ContainsKey("code"))
                    {
                        string code = parameters["code"].ToString();
                        interpreter.Execute(code);
                    }
                    response.Data = new { message = "Execution started/completed" };
                    break;
                    
                case "ExecuteSql":
                    var rows = new List<object>();
                    for(int i=0; i<15; i++) {
                        rows.Add(new {
                            id = i + 1,
                            name = $"Entity {(char)('A' + (i % 26))}{i + 1}",
                            sales = new Random().Next(2000, 10000),
                            expenses = new Random().Next(1000, 6000),
                            created_at = DateTime.Now.AddDays(-new Random().Next(1, 100)).ToString("yyyy-MM-dd")
                        });
                    }
                    var columns = new List<object> {
                        new { field = "id", header = "ID" },
                        new { field = "name", header = "Name" },
                        new { field = "sales", header = "Sales ($)" },
                        new { field = "expenses", header = "Expenses ($)" },
                        new { field = "created_at", header = "Date" }
                    };
                    
                    response.Data = new { rows = rows, columns = columns };
                    break;
                    
                case "SaveScript":
                    if (parameters.ContainsKey("script"))
                    {
                        var scriptObj = JObject.FromObject(parameters["script"]);
                        var id = scriptObj["id"]?.ToString();
                        
                        var existing = _userScripts[Uuid].FirstOrDefault(s => s["id"]?.ToString() == id);
                        if (existing != null)
                        {
                            _userScripts[Uuid].Remove(existing);
                        }
                        _userScripts[Uuid].Add(scriptObj);
                    }
                    break;
                    
                case "LoadScripts":
                    string lang = parameters.ContainsKey("language") ? parameters["language"].ToString() : "";
                    var filtered = _userScripts[Uuid].Where(s => s["language"]?.ToString() == lang || (lang == "sql" && s["database"] != null)).ToList();
                    response.Data = filtered;
                    break;
                    
                case "DeleteScript":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var existing = _userScripts[Uuid].FirstOrDefault(s => s["id"]?.ToString() == id);
                        if (existing != null)
                        {
                            _userScripts[Uuid].Remove(existing);
                        }
                    }
                    break;

                case "SaveDatabaseConnection":
                    if (parameters.ContainsKey("connection"))
                    {
                        var connObj = JObject.FromObject(parameters["connection"]);
                        var id = connObj["id"]?.ToString();
                        
                        var existing = _userDatabaseConnections[Uuid].FirstOrDefault(c => c["id"]?.ToString() == id);
                        if (existing != null)
                        {
                            _userDatabaseConnections[Uuid].Remove(existing);
                        }
                        _userDatabaseConnections[Uuid].Add(connObj);
                    }
                    break;
                    
                case "LoadDatabaseConnections":
                    response.Data = _userDatabaseConnections[Uuid].ToList();
                    break;
                    
                case "DeleteDatabaseConnection":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var existing = _userDatabaseConnections[Uuid].FirstOrDefault(c => c["id"]?.ToString() == id);
                        if (existing != null)
                        {
                            _userDatabaseConnections[Uuid].Remove(existing);
                        }
                    }
                    break;
                    
                case "TestDatabaseConnection":
                    // Mock random connection test result just like the frontend did
                    bool success = new Random().NextDouble() > 0.3;
                    response.Data = new { success = success };
                    break;
                    
                case "SaveDataset":
                    if (parameters.ContainsKey("dataset"))
                    {
                        var dsObj = JObject.FromObject(parameters["dataset"]);
                        var id = dsObj["id"]?.ToString();
                        if (string.IsNullOrEmpty(id))
                        {
                            id = Guid.NewGuid().ToString();
                            dsObj["id"] = id;
                        }
                        
                        var existing = _userDatasets[Uuid].FirstOrDefault(d => d["id"]?.ToString() == id);
                        if (existing != null) _userDatasets[Uuid].Remove(existing);
                        _userDatasets[Uuid].Add(dsObj);
                    }
                    break;

                case "LoadDatasets":
                    response.Data = _userDatasets[Uuid].ToList();
                    break;

                case "DeleteDataset":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var existing = _userDatasets[Uuid].FirstOrDefault(d => d["id"]?.ToString() == id);
                        if (existing != null) _userDatasets[Uuid].Remove(existing);
                    }
                    break;

                case "SaveExcel":
                    if (parameters.ContainsKey("excel"))
                    {
                        var exObj = JObject.FromObject(parameters["excel"]);
                        var id = exObj["id"]?.ToString();
                        if (string.IsNullOrEmpty(id))
                        {
                            id = Guid.NewGuid().ToString();
                            exObj["id"] = id;
                        }
                        
                        var existing = _userExcels[Uuid].FirstOrDefault(d => d["id"]?.ToString() == id);
                        if (existing != null) _userExcels[Uuid].Remove(existing);
                        _userExcels[Uuid].Add(exObj);
                    }
                    break;

                case "LoadExcels":
                    response.Data = _userExcels[Uuid].ToList();
                    break;

                case "DeleteExcel":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var existing = _userExcels[Uuid].FirstOrDefault(d => d["id"]?.ToString() == id);
                        if (existing != null) _userExcels[Uuid].Remove(existing);
                    }
                    break;

                case "SaveDashboard":
                    if (parameters.ContainsKey("dashboard"))
                    {
                        var dashObj = JObject.FromObject(parameters["dashboard"]);
                        var id = dashObj["id"]?.ToString();
                        if (string.IsNullOrEmpty(id))
                        {
                            id = Guid.NewGuid().ToString();
                            dashObj["id"] = id;
                        }
                        
                        var existing = _userDashboards[Uuid].FirstOrDefault(d => d["id"]?.ToString() == id);
                        if (existing != null) _userDashboards[Uuid].Remove(existing);
                        _userDashboards[Uuid].Add(dashObj);
                    }
                    break;

                case "LoadDashboards":
                    response.Data = _userDashboards[Uuid].ToList();
                    break;

                case "DeleteDashboard":
                    if (parameters.ContainsKey("id"))
                    {
                        var id = parameters["id"].ToString();
                        var existing = _userDashboards[Uuid].FirstOrDefault(d => d["id"]?.ToString() == id);
                        if (existing != null) _userDashboards[Uuid].Remove(existing);
                    }
                    break;

                default:
                    response.Status = MessageStatus.Error;
                    response.ErrorMessage = "Unknown command";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.Status = MessageStatus.Error;
            response.ErrorMessage = ex.Message;
        }

        string msg = MessageSerializer.Serialize(response);
        Message(msg);
    }
    
    private void ErrorOccurred(object sender, Exception e)
    {
        Console.WriteLine(e.Message);
    }

    private void AuthenticationMessage(object sender, MessageReceivedEventArgs e)
    {
        dynamic data = new JObject();
         data.Uuid = Uuid;
         data.Menu = new JObject();
         data.Menu.Header = "";
         data.Functions = new JArray(   interpreter.GetFunctions());
        ResponseMessage responde = new ResponseMessage
        {
            Status = MessageStatus.Success,
            ErrorMessage ="",
            Data = data
        };

        string msg = JsonConvert.SerializeObject(responde);
        Message(msg);
    }
    
    private void TestScriptOnStatusUpdate(object sender, StatusString e)
    {
        NotificationMessage notification = new NotificationMessage
        {
            Category = "Debug",
            Content = e.status,
            Title = "Execution Debug"
        };
        string msg = MessageSerializer.Serialize(notification);
        Message(msg);
    }

    public void OnMessage(string json)
    {
        _clientMsg.ReceiveMsg(json);
    }
   
    private void Message (string message)
    {
        Socket.Send(message);
    }
}