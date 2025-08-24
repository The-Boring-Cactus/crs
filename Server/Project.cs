using System.Text.Json;
using GenHTTP.Api.Content;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Websockets;
using Newtonsoft.Json.Linq;

namespace Server;

public static class Project
{
    private static List<Client> _AllSockets = [];

    public static IHandlerBuilder Setup()
    {
        var files = Resources.From(ResourceTree.FromDirectory("Resources")); 

        // see https://genhttp.org/documentation/content/frameworks/websockets/
        var socket = Websocket.Create()
                              .OnOpen((socket) =>
                              {
                                  Console.WriteLine("Open!");
                                  var client = Guid.NewGuid().ToString();
                                  Console.WriteLine(client);
                                  Console.WriteLine(_AllSockets.Count.ToString());
                                  dynamic answer = new JObject();
                                  answer.TypeMsg ="Welcome";
                                  answer.TypeAction = "";
                                  answer.data = client;

                                  AuthenticationMessage ss = new AuthenticationMessage();

                                  string ssJ = JsonSerializer.Serialize(ss);
                                  Console.WriteLine(ssJ);
                                  
                                  var sz = answer.ToString();
                                  Console.WriteLine(sz);
                                  socket.Send(sz);
                                  var s = new Client(socket, client);
                                  _AllSockets.Add(s);
                              })
                              .OnClose((socket) =>
                              {
                                  var s = _AllSockets.FirstOrDefault(s => s.Socket == socket);
                                  _AllSockets.Remove(s );
                              })
                              .OnMessage((socket, message) =>
                              {
                                  
                                  var s = _AllSockets.FirstOrDefault(s => s.Socket == socket);
                                  s.OnMessage(message);
                              });

        return Layout.Create()
            .Add("/",files)
            .Add("srv", socket); // ws://localhost:8080/srv/
                    
    }

}
