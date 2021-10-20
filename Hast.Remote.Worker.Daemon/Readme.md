# Hastlayer - Remote Worker Daemon


Service to run the Hastlayer Worker.


## Server Setup

### Configuration

You can edit the _appsettings.json_ file after build if you wish. When calling the app directly (e.g. during development or experimentation) use [user secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows). Use environment variables in production.

To add user secrets go to the project directory and type commands below with the correct values. If are using [Azurite](https://github.com/Azure/Azurite) for local Azure Storage emulation you can omit the second line.
```shell
dotnet user-secrets set "ApplicationInsights:InstrumentationKey" "YOUR AI INSTRUMENTATION KEY"
dotnet user-secrets set "ConnectionStrings:Hast.Remote.Worker.Daemon.StorageConnectionString" "YOUR STORAGE CONNECTION STRING"
```

To use environment variables first you have to delete the _appsettings.json_ file (or at least these two corresponding entries).
- On Windows type these. The `/M` switch sets the variables for the whole machine because the service is running as a different service-specific user.
  - `setx "APPINSIGHTS_INSTRUMENTATIONKEY" "YOUR AI INSTRUMENTATION KEY" /M`
  - `setx "STORAGE_CONNECTIONSTRING" "YOUR STORAGE CONNECTION STRING" /M`
- On Linux you have to edit the service after it's created by appending `Environment=` lines as described [here](https://www.freedesktop.org/software/systemd/man/systemd.exec.html#Environment=).

Note that you don't need the entire Application Insights connection string, just the GUID in the _InstrumentationKey_ field.
For further documentation take a look at the `ConfigurationKeys` class.


### Installation

If you're upgrading from an existing version, do the following. On Windows you can use the _HastlayerRemoteWorkerDaemonInstallHelper.bat_ to quickly swap to a newer directory without following the rest of the Installation section. If not, you can skip these.
1. Make sure that the Services window is closed (otherwise you'll get the "The specified service has been marked for deletion." error).
2. Type `Hast.Remote.Worker.Daemon.exe cli uninstall`. (You could use stop instead of uninstall too, but the service install/uninstall is really fast so it's not worth it.)
3. Delete the application's directory or move it to a new location for archival. If you get an error that it's in use, wait until the service fully terminates.

Now that you have a clean slate:
1. Publish the project in Release mode.
2. Copy the folder with the service executable onto the server in a desired location (e.g. _C:\HastlayerRemoteWorkerDaemon_).
3. Open a terminal as administrator and go to the same folder. 
4. Type `Hast.Remote.Worker.Daemon.exe cli install`. This will install and start the service. Verify that the installation was successful by checking Services.

5. Check the logs for any issues.

You can install, uninstall, start and stop the service by typing `Hast.Remote.Worker.Daemon.exe cli [command]`. The install and uninstall also starts and stops the service respectively.


### Docker Installation

1. Edit the source _appsettings.json_ file in the project directory.
2. Type `docker build -t hast-remote-worker-daemon -f Hast.Core/Hast.Remote.Worker.Daemon/Dockerfile .` into bash or powershell.
3. Create a container using [`docker run`](https://docs.docker.com/engine/reference/run/) (or a frontend such as [Docker Desktop](https://www.docker.com/products/docker-desktop) or [DockStation](https://dockstation.io/))
4. If you run on a remote server [export](https://docs.docker.com/engine/reference/commandline/export/) the container so you can deploy it on your target machine.

## Logging

The service writes log messages during start and stop to the Windows event log. You can view the entries in the Windows Event Viewer under "Applications and Services Logs" in the log "Hastlayer Remote Worker Daemon". Once the service is running the standard `Microsoft.Extensions.Logging` logs will be used.
