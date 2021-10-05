@echo off
rem Script for automating the folder creation/rename, service start/stop workflow of installing the Hastlayer Remote
rem Worker Daemon. Will use conventional folder names.

if not exist "HastlayerRemoteWorkerDaemonNew" mkdir HastlayerRemoteWorkerDaemonNew

echo The HastlayerRemoteWorkerDaemonNew folder created. Now copy the Daemon's Release folder's content (from within the runtime's folder, e.g. win-x64) there and press enter.

pause > nul

echo Copying config files from the original installation.

copy /Y HastlayerRemoteWorkerDaemon\appsettings.json HastlayerRemoteWorkerDaemonNew\appsettings.json

echo Config files from the original installation copied. Change them now if necessary. Then press enter when you're done and the service instances will be swapped out.

pause > nul

@echo on
net stop "Hast.Remote.Worker.Daemon"
@echo off

echo Swapping service instances.
rename HastlayerRemoteWorkerDaemon HastlayerRemoteWorkerDaemonOld
rename HastlayerRemoteWorkerDaemonNew HastlayerRemoteWorkerDaemon

@echo on
net start "Hast.Remote.Worker.Daemon"

@echo Service instances swapped and the new instance started. If you want to swap back just rename the HastlayerRemoteWorkerDaemonOld folder to HastlayerRemoteWorkerDaemonNew and run this script again. Now check the new service instance's logs to see if everything is all right and test the service.
