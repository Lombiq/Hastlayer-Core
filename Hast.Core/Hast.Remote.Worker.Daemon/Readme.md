# Hastlayer - Remote Worker Daemon readme



Windows service to run the Hastlayer Worker.


## Installation on the server

1. Build the project in Release mode and copy the service exe and its dependencies (basically the whole Release bin folder) onto the server in a desired location (e.g. *C:\HastlayerRemoteWorkerDaemon*). If you just update the Worker then you may copy the last modified files only.
2. Configure the service in the *Hast.Remote.Worker.Daemon.exe.config* file (for documentation on these take a look at the `ConfigurationKeys` class). If you're updating the service on an already running server then make sure not to overwrite the config file to keep the settings.
3. Run the exe as administrator. This will install the service (running it again uninstalls it). Verify if the installation was successful by checking Services.
4. The service is set to automatic start, i.e. it will start with Windows. The first time however it should be manually started from Services.


## Logging

The service writes log messages during start and stop to the Windows event log. You can view the entries in the Windows Event Viewer under "Applications and Services Logs" in the log "Hastlayer Remote Worker Daemon". Once the service is running the standard Orchard logs will be used (note that the Daemon starts a standard Hastlayer shell which will run its own Orchard host and have its own log files).