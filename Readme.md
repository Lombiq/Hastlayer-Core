# Hastlayer Readme



## Overview

[Hastlayer](http://www.hastlayer.com/) - Be the hardware. Transforming .NET assemblies into FPGA hardware for faster execution. This is the PC-side component of Hastlayer, the one that transforms .NET assemblies, programs attached FPGAs and communicates with said FPGAs.

On how to use Hastlayer in your own application see the sample projects in the solution.

Created by [Lombiq Technologies](https://lombiq.com/). 

Hastlayer uses [ILSpy](http://ilspy.net/) to process CIL assemblies and [Orchard Application Host](https://github.com/Lombiq/Orchard-Application-Host) to utilize [Orchard](http://orchardproject.net/) as the application framework.


## Writing Hastlayer-compatible .NET code

Take a look at the sample projects in the Sample solution folder. Those are there to give you a general idea how Hastlayer-compatible code looks like, and they're thoroughly documented.

Some general constraints you have to keep in mind:

- Only public virtual methods, or methods that implement a method defined in an interface will be accessible from the outside, i.e. can be hardware entry points.
- Always use the smallest data type necessary, e.g. `short` instead of `int` if 16b is enough (or even `byte`), and unsigned types like `uint` if you don't need negative numbers.
- Supported primitive types: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `char`, `bool`.  Floating-point numbers like `float` and `double` and numbers bigger than 64b are not yet supported.
- The most important language constructs like `if` and `else` statements, `while` and `for` loops, type casting, binary operations (e.g. arithmetic, in/equality operators...), conditional expressions (ternary operator) on allowed types are supported.
- Algorithms can use a fixed-size (determined at runtime) memory space modelled as a `byte` array in the class `SimpleMemory`. For inputs that should be passed to hardware implementations and outputs that should be sent back this memory space is to be used. For internal method arguments (i.e. for data that isn't coming from the host computer or should be sent back) normal method arguments can be used.
- Single-dimensional arrays having their size possible to determine compile-time are supported. So apart from instantiating arrays with their sizes specified as constants you can also use variables, fields, properties for array sizes, as well as expressions (and a combination of these), just in the end the size of the array needs to be resolvable at compile-time. To a limited degree `Array.Copy()` is also supported: only the `Copy(Array sourceArray, Array destinationArray, int length)` override and only with a constant `length`.
- Using objects created of custom classes are supported. Using these objects as usual (e.g. passing them as method arguments, storing them in arrays) is also supported. However hardware entry point types can only contain methods.
- Task-based parallelism is with TPL is supported to a limited degree. Lambda expression are supported to an extent needed to use tasks (see samples).
- Operation-level, SIMD-like parallelism is supported, see samples.

See the samples to get an understanding of what you can do in Hastlayer-compatible .NET code.


## Troubleshooting

If any error happens during runtime Hastlayer will throw an exception (mostly but not exclusively a `HastlayerException`) and the error will be also logged. Log files are located in the `App_Data\Logs` folder under your app's execution folder.


## Extensibility

Hastlayer, apart from the standard Orchard-style extensibility (e.g. the ability to override implementations of services through the DI container) provides three kind of extension points:

- .NET-style events: standard .NET events.
- Orchard-style events: event handlers that can be hooked into by implementing the event handler interface.
- Pipeline steps: unlike event handlers, pipeline steps are executed in deterministic order and usually have a return value that is fed to the next pipeline step.


## Design principles

- From a user's (i.e. using developer's) perspective Hastlayer should be as simple as possible. To achieve this e.g. use generally good default configurations so in the majority of cases there is no configuration needed.
- Software that was written previously, without knowing about Hastlayer should be usable if it can live within the constraints of transformable code. E.g. users should never be forced to use custom attributes or other Hastlayer-specific elements in their code if the same effect can be achived with runtime configuration (think about how members to be processed are configured: when running Hastlayer, not with attributes).