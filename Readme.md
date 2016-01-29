# Hastlayer Readme



## Overview

[Hastlayer](http://www.hastlayer.com/) - Be the hardware. Transforming .NET assemblies into FPGA hardware for faster execution.

On how to use Hastlayer in your own application see the sample projects.

Created by [Lombiq Technologies](http://lombiq.com/). 

Hastlayer uses [ILSpy](http://ilspy.net/) to process CIL assemblies and [Orchard Application Host](http://orchardapphost.codeplex.com/) to utilize [Orchard](http://orchard.codeplex.com/) as the application framework.


## Writing Hastlayer-compatible .NET code

Take a look at the sample projects in the Sample solution folder. Those are there to give you a general idea how Hastlayer-compatible code looks like, and they're thoroughly documented.

Some general constraints you have to keep in mind:

- Only public virtual methods, or methods that implement a method defined in an interface will be accessible from the outside.
- Always use the smallest data type necessary, e.g. `short` instead of `int` if 16b is enough, and unsigned types like `uint` if you don't need negative numbers.
- Floating-point numbers like `float` and `double` and numbers bigger than 32b are not supported.
- Algorithms can use a fixed-size (determined at runtime) memory space modelled as a `byte` array in the class `SimpleMemory`. For inputs that should be passed to hardware implementations and outputs that should be sent back this memory space is to be used. Internal method arguments (i.e. for data that isn't coming from the host computer or should be sent back) normal method arguments can be used.
- Passing object references is not supported.


## Troubleshooting

If any error happens during runtime Hastlayer will throw an exception (mostly but not exclusively a `HastlayerException`) and the error will be also logged. Log files are located in the `App_Data\Logs` folder under your app's execution folder.


## Extensibility

Hastlayer, apart from the standard Orchard-style extensibility (e.g. the ability to override implementations of services through the DI container) provides three kind of extension points:

- .NET-style events: standard .NET events.
- Orchard-style events: event handlers that can be hooked into by implementing the event handler interface.
- Pipeline steps: unlike event handlers, pipeline steps are executed in deterministic order and usually have a return value that is fed to the next pipeline step.