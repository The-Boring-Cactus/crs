using FunctEngine;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Server;

public class Client
{

    public IWebsocketConnection Socket;
    private CodeEngine interpreter = new CodeEngine();
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

    private void CommandMessage(object sender, MessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
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
        dynamic answer = new JObject();
        answer.TypeMsg ="Debug";
        answer.data = e.status;;
        var sz = answer.ToString();
        Message(sz);
    }

    public void OnMessage(string json)
    {
        
        _clientMsg.ReceiveMsg(json);
        
      /*  
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
            
        }*/
      
    }
   
    private void Message (string message)
    {
        Socket.Send(message);
    }
}