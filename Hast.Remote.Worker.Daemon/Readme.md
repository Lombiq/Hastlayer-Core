# Hastlayer - Remote Worker Daemon



Windows service to run the Hastlayer Worker.


## Installation on the server

First build the project in Release mode. Then according to your case:

### If this is the first installation
1. Copy the service exe and its dependencies (basically the whole Release bin folder) onto the server in a desired location (e.g. *C:\HastlayerRemoteWorkerDaemon*).
2. Configure the service in the *Hast.Remote.Worker.Daemon.exe.config* file (for documentation on these take a look at the `ConfigurationKeys` class) and the Application Insights instrumentation key in *ApplicationInsights.config*.
3.  Run the service exe as administrator. This will install the service (running it again uninstalls it). Verify that the installation was successful by checking Services.
4. Start the service. The service is set to automatic start, i.e. it will start with Windows but after installation however it should be manually started from Services.
5. Check the logs for any issues.

### If you're updating the service
To minimize downtime and have the ability to roll back to the previously installed version of the service do the below steps, or copy the `HastlayerRemoteWorkerDaemonInstallHelper.bat` script to the installation folder (i.e. the root folder where you want the Daemon's folder to be created, e.g. *C:\\*) and run it, follow its directions (no other steps needed before that).

1. Copy the service exe and its dependencies (basically the whole Release bin folder, within the runtime's folder, e.g. *win-x64*) onto the server in a desired temporary location (e.g. *C:\HastlayerRemoteWorkerDaemonNew*).
2. Configure the service in the *appsettings.json* file (for documentation on these take a look at the `ConfigurationKeys` class) and the Application Insights instrumentation key there too. You can copy the configuration files of the running service over too, if the configuration formats haven't changed.
3. Stop the running service.
4. Rename the folder of the running service (eg. *HastlayerRemoteWorkerDaemonOld*), then name the temporary folder of the new instance to its original name (eg. *HastlayerRemoteWorkerDaemon*).
5. Restart the service.
6. Check the logs for any issues.
7. If everything is alright, remove the old service instance's folder.


## Logging

The service writes log messages during start and stop to the Windows event log. You can view the entries in the Windows Event Viewer under "Applications and Services Logs" in the log "Hastlayer Remote Worker Daemon". Once the service is running the standard Microsoft.Extensions.Logging logs will be used.
