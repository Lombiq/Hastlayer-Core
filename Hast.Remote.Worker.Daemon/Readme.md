# Hastlayer - Remote Worker Daemon readme



Windows service to run the Hastlayer Worker.


## Installation on the server

1. If the service is already running then stop it first.
2. Build the project in Release mode and copy the service exe and its dependencies (basically the whole Release bin folder) onto the server in a desired location (e.g. *C:\HastlayerRemoteWorkerDaemon*). If you just update the Worker then you may copy the last modified files only.
3. Configure the service in the *Hast.Remote.Worker.Daemon.exe.config* file (for documentation on these take a look at the `ConfigurationKeys` class). If you're updating the service then make sure not to overwrite the config file to keep the settings or re-add them.
4. If you're updating the service but there was no Hastlayer version number change since the last deployment despite transformation logic changing (i.e. you do a rolling update) then also make sure to remove any cache files Hastlayer may have created. These are under *[Daemon installation directory]\Hastlayer\App_Data\Hastlayer*.
5. If you're installing the service the first time then run the exe as administrator. This will install the service (running it again uninstalls it). Verify that the installation was successful by checking Services.
6. (Re-)start the service. The service is set to automatic start, i.e. it will start with Windows but after installation however it should be manually started from Services.


## Logging

The service writes log messages during start and stop to the Windows event log. You can view the entries in the Windows Event Viewer under "Applications and Services Logs" in the log "Hastlayer Remote Worker Daemon". Once the service is running the standard Orchard logs will be used (note that the Daemon starts a standard Hastlayer shell which will run its own Orchard host and have its own log files).