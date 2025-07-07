using FunctEngine;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json;

namespace reporter;

public class Client
{
    private GlobalVariables globalVariables ;
    public IWebsocketConnection socket;
    
    private StatementEvaluator statementEvaluator;
    private FunctList mapList;
    private FunctScript testScript ;
    public string uuid ;
    public Client (IWebsocketConnection socket, string _uuid)
    {
        this.socket = socket;
        this.uuid = _uuid;
        globalVariables = new GlobalVariables();
        statementEvaluator = new StatementEvaluator();
         mapList = new FunctList();
         testScript = new FunctScript(mapList, statementEvaluator, globalVariables);
         testScript.StatusUpdate += TestScriptOnStatusUpdate;
         
         
    }
    

    private void TestScriptOnStatusUpdate(object sender, StatusString e)
    {
        dynamic msg = new System.Dynamic.ExpandoObject();
        msg.type = "Debug";
        msg.text = e.status;
        var j = JsonConvert.SerializeObject(msg);
        Message(j);
    }

    public void Message (string message)
    {
        dynamic msg = new System.Dynamic.ExpandoObject();
        msg.type = "Debug";
        msg.text =message;
        var j = JsonConvert.SerializeObject(msg);
        
        Console.WriteLine($"get message from {uuid}");
        Console.WriteLine($"get message from {j}");
        Console.WriteLine(j);
        socket.Send(j);
    }
}