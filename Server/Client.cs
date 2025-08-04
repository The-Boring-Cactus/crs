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
        mapList = new FunctList();
        globalVariables = new GlobalVariables();
        statementEvaluator = new StatementEvaluator();
        testScript = new FunctScript(mapList, statementEvaluator, globalVariables);
        testScript.StatusUpdate += TestScriptOnStatusUpdate;
        testScript.StatusFinish += TestScriptOnStatusFinish;

    }
    private void TestScriptOnStatusFinish(object sender, StatusString e)
    {
        dynamic answer = new JObject();
        answer.TypeMsg ="FinishCode";
        answer.status = e.status;
        answer.data = JObject.FromObject(testScript.FunctResultsList);
        var sz = answer.ToString();
        Message(sz);
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
        string type = payload.type;
        switch (type)
        {
            case "Login" :
                dynamic answer = new JObject();
                answer.TypeMsg = "Login";
                answer.data = new JObject();
                answer.data.level = "Admin";
                answer.data.auth = true;
                var sz = answer.ToString();
                Message(sz); 
                break;
            case "CodeScript":
                globalVariables = new GlobalVariables();
                statementEvaluator = new StatementEvaluator();
                testScript = new FunctScript(mapList, statementEvaluator, globalVariables);
                testScript.StatusUpdate += TestScriptOnStatusUpdate;
                testScript.StatusFinish += TestScriptOnStatusFinish;
                List<CompilerError> compilerErrors = new List<CompilerError>();
                string script = payload.data;
                string name = payload.name;
                bool isok = testScript.InitializeScript(script, name, ref compilerErrors);
                testScript.StartFunct();
                break;
            
        }
    }
    public void Message (string message)
    {
        socket.Send(message);
    }
}