using GenHTTP.Api.Content;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Websockets;

namespace reporter;

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
                                  socket.Send(client);
                                  var s = new Client(socket, client);
                                  _AllSockets.Add(s);
                              })
                              .OnClose((socket) =>
                              {
                                  Console.WriteLine("Close!");
                                  var s = _AllSockets.FirstOrDefault(s => s.socket == socket);
                                  _AllSockets.Remove(s );
                              })
                              .OnMessage((socket, message) =>
                              {
                                  Console.WriteLine(socket);
                                  Console.WriteLine(message);
                                  var s = _AllSockets.FirstOrDefault(s => s.socket == socket);
                                  s.Message(message);
                              });

        return Layout.Create()
            .Add("/",files)
            .Add("srv", socket); // ws://localhost:8080/srv/
                    
    }

}
