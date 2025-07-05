using GenHTTP.Engine.Internal;

using GenHTTP.Modules.Practices;

using reporter;

// run this sample and open http://localhost:8080 in your browser

var project = Project.Setup();
return await Host.Create()
                 .Handler(project)
                 .Defaults()
                 .Console()
#if DEBUG
                 .Development()
#endif
                 .RunAsync();
