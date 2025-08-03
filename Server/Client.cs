using FunctEngine;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server;

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
        uuid = _uuid;
        globalVariables = new GlobalVariables();
        statementEvaluator = new StatementEvaluator();
         mapList = new FunctList();
         testScript = new FunctScript(mapList, statementEvaluator, globalVariables);
         testScript.StatusUpdate += TestScriptOnStatusUpdate;
         
         
    }
    
    private void TestScriptOnStatusUpdate(object sender, StatusString e)
    {
        dynamic answer = new JObject();
        answer.TypeMsg ="Debug";
        answer.data = e.status;;
        var sz = answer.ToString();
        Message(sz);
    }

    public void OnMessage(string message)
    {
        dynamic payload = JObject.Parse(message);
       
            if (payload.type == "login")
            {
                
                dynamic answer = new JObject();
                answer.TypeMsg ="Login";
                answer.data = new JObject();
                answer.data.level="Admin";
                answer.data.auth = true;
                var sz = answer.ToString();
                
                
                Message(sz);
            }
       
    }
    public void Message (string message)
    {
        socket.Send(message);
    }
}