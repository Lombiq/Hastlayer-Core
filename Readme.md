# Hastlayer Readme



## Overview

[Hastlayer](http://www.hastlayer.com/) - Be the hardware. Transforming .NET assemblies into FPGA hardware for faster execution.

On how to use Hastlayer in your own application see the sample projects.

Created by [Lombiq Technologies](http://lombiq.com/). 

Hastlayer uses [ILSpy](http://ilspy.net/) to process CIL assemblies and [Orchard Application Host](http://orchardapphost.codeplex.com/) to utilize [Orchard](http://orchard.codeplex.com/) as the application framework.


## Troubleshooting

If any error happens during runtime Hastlayer will throw an exception (mostly but not exclusively a `HastlayerException`) and the error will be also logged. Log files are located in the `App_Data\Logs` folder under your app's execution folder.


## Extensibility

Hastlayer, apart from the standard Orchard-style extensibility (e.g. the ability to override implementations of services through the DI container) provides three kind of extension points:

- .NET-style events: standard .NET events.
- Orchard-style events: event handlers that can be hooked into by implementing the event handler interface.
- Pipeline steps: unlike event handlers, pipeline steps are executed in deterministic order and usually have a return value that is fed to the next pipeline step.