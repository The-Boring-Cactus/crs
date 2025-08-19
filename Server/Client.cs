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

    public IWebsocketConnection socket;
    
    
    public string uuid ;
    public Client (IWebsocketConnection socket, string _uuid)
    {
        this.socket = socket;
        uuid = _uuid;
        
     
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
                var interpreter = new CodeEngine();
                //interpreter.LoadExternalDll("MathFunctions.dll");
                //interpreter.LoadExternalDll("StringUtilities.dll");
                interpreter.StatusUpdate += TestScriptOnStatusUpdate;
                string script = payload.data;
                interpreter.Execute(script);
                
                break;
            
        }
    }
    public void Message (string message)
    {
        socket.Send(message);
    }
}