# Hastlayer - Remote Worker Console



Simple console app to run the Hastlayer Worker.



## Requirements

* An [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource) app.
* [Azurite](https://github.com/azure/azurite) which works on any platform with [node](https://nodejs.org/) or [Docker](https://hub.docker.com/_/microsoft-azure-storage-azurite). (note: the formerly official [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) has been deprecated)



## Usage Notes

Place your [instrumentation key](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource#copy-the-instrumentation-key) in the appsettings.json file or the APPINSIGHTS_INSTRUMENTATIONKEY environmental variable. For further details see [here](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core).
