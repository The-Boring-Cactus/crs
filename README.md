# Cactus Reporter

Reporte system to view some graphs using a weird C# similar language

## Development

To build this project from source, checkout this repository and execute
the following commands in your terminal. This requires the [node 24.3.0]
(https://nodejs.org/en/download/) and [.NET SDK](https://dotnet.microsoft.com/download) 
to be installed.
The database (in the future I'll chnage but no now) use in this project
 [orientdb](https://orientdb.dev/) install it.

```
cd ui
npm build
cd ..
dotnet run
```

This will make the service available at http://localhost:8080/.

## About

This project uses the [GenHTTP webserver](https://genhttp.org/) to
implement its functionality.
