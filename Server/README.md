# Cactus Reporter

Reporte system to view some graphs using a weird C# similar language

## Development

To build this project from source, checkout this repository and execute
the following commands in your terminal. This requires the [node 24.3.0](https://nodejs.org/en/download/) and [.NET SDK](https://dotnet.microsoft.com/download) 
to be installed.
This application use [surrealdb](https://github.com/surrealdb/surrealdb?tab=readme-ov-file#installation) (in the future I'll change but no now) use in this project
 install it.

## DB install in Linux
```
curl --proto '=https' --tlsv1.2 -sSf https://install.surrealdb.com | sh
```
## DB install in Windows
```
iwr https://windows.surrealdb.com -useb | iex
```

## Build the UI
```
cd ui
npm build
```
## Run the app
```
dotnet run
```

This will make the service available at http://localhost:8080/.

## About

This project uses the [GenHTTP webserver](https://genhttp.org/) to
implement its functionality.
