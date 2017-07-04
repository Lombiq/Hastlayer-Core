# Hastlayer Readme



## Overview

[Hastlayer](http://hastlayer.com/) - Be the hardware. Transforming .NET assemblies into FPGA hardware for faster execution. This is the PC-side component of Hastlayer, the one that transforms .NET assemblies, programs attached FPGAs and communicates with said FPGAs.

On how to use Hastlayer in your own application see the sample projects in the solution.

Created by [Lombiq Technologies](https://lombiq.com/).

Hastlayer uses [ILSpy](http://ilspy.net/) to process CIL assemblies and [Orchard Application Host](https://github.com/Lombiq/Orchard-Application-Host) to utilize [Orchard](http://orchardproject.net/) as the application framework.


## Writing Hastlayer-compatible .NET code

Take a look at the sample projects in the Sample solution folder. Those are there to give you a general idea how Hastlayer-compatible code looks like, and they're thoroughly documented. The `PrimeCalculator` is a good starting point with a basic sample algorithm.

Some general constraints you have to keep in mind:

- Only public virtual methods, or methods that implement a method defined in an interface will be accessible from the outside, i.e. can be hardware entry points.
- Always use the smallest data type necessary, e.g. `short` instead of `int` if 16b is enough (or even `byte`), and unsigned types like `uint` if you don't need negative numbers.
- Supported primitive types: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `char`, `bool`.  Floating-point numbers like `float` and `double` and numbers bigger than 64b are not yet supported.
- The most important language constructs like `if` and `else` statements, `while` and `for` loops, type casting, binary operations (e.g. arithmetic, in/equality operators...), conditional expressions (ternary operator) on allowed types are supported.
- Algorithms can use a fixed-size (determined at runtime) memory space modeled as a `byte` array in the class `SimpleMemory`. For inputs that should be passed to hardware implementations and outputs that should be sent back this memory space is to be used. For internal method arguments (i.e. for data that isn't coming from the host computer or should be sent back) normal method arguments can be used. Note that there shouldn't be concurrent access to a `SimpleMemory` instance, it's **not** thread-safe (neither in software nor on hardware)!
- Single-dimensional arrays having their size possible to determine compile-time are supported. So apart from instantiating arrays with their sizes specified as constants you can also use variables, fields, properties for array sizes, as well as expressions (and a combination of these), just in the end the size of the array needs to be resolvable at compile-time. To a limited degree `Array.Copy()` is also supported: only the `Copy(Array sourceArray, Array destinationArray, int length)` override and only with a constant `length`. Furthermore, `ImmutableArray` is also supported to a limited degree by converting objects of that type to standard arrays in the background.
- Using objects created of custom classes and structs are supported. Using these objects as usual (e.g. passing them as method arguments, storing them in arrays) is also supported. However hardware entry point types can only contain methods. Also, be careful not to mix reference types (like arrays) into structs' members (fields and properties), keep structs purely value types (this is a good practice any way).
- Task-based parallelism is with TPL is supported to a limited degree. Lambda expression are supported to an extent needed to use tasks (see the `ParallelAlgorithm` sample).
- Operation-level, SIMD-like parallelism is supported, see the `SimdCalculator` sample.
- Recursion is supported but recursive code is not really something for Hastlayer. Nevertheless if a method call is recursive, even if indirectly, you need to manually configure the recursion depths (see the `RecursiveAlgorithms` sample).
- Note that you can write unsupported code in a member of a type that will be transformed if that member won't be accessed on the hardware (since unused code is removed from transformation). So e.g. you can implement `ToString()`.


## Performance-optimizing your code

Some simplified basics first on the properties of FPGAs first:

- FPGAs are low-power devices running on small clock frequencies (few 100Mhz at most), so we need to be cautious with clock cycles.
- On an FPGA you can do a lot of simpler operations (like variable assignments, arithmetic on smaller numbers) in a single clock cycle even without parallelization.
- However it's only useful to look at FPGAs for performance enhancements if your code can be massively parallelized on a `Task` level.

So to write fast code with Hastlayer you need implement massively parallel algorithms and avoid code that adds unnecessary clock cycles. What are the clock cycle sinks to avoid?

- Method invocation and access to custom properties (i.e. properties that have a custom getter or setter, so not auto-properties) cost multiple clock cycles as a baseline. Try to avoid having many small methods (Hastlayer will eventually inline small methods to cut down on such waste automatically) and custom properties.
- Arithmetic operations take longer with larger number types so always use the smallest data type necessary (e.g. use `short` instead of `int` if its range is enough).
- Memory access with `SimpleMemory` is relatively slow, so keep memory access to the minimum (use local variables and objects as temporary storage instead).


## Troubleshooting

If any error happens during runtime Hastlayer will throw an exception (mostly but not exclusively a `HastlayerException`) and the error will be also logged. Log files are located in the `App_Data\Logs` folder under your app's execution folder.


## Extensibility

Hastlayer, apart from the standard Orchard-style extensibility (e.g. the ability to override implementations of services through the DI container) provides three kind of extension points:

- .NET-style events: standard .NET events.
- Orchard-style events: event handlers that can be hooked into by implementing the event handler interface.
- Pipeline steps: unlike event handlers, pipeline steps are executed in deterministic order and usually have a return value that is fed to the next pipeline step.


## Design principles

- From a user's (i.e. using developer's) perspective Hastlayer should be as simple as possible. To achieve this e.g. use generally good default configurations so in the majority of cases there is no configuration needed.
- Software that was written previously, without knowing about Hastlayer should be usable if it can live within the constraints of transformable code. E.g. users should never be forced to use custom attributes or other Hastlayer-specific elements in their code if the same effect can be achieved with runtime configuration (think about how members to be processed are configured: when running Hastlayer, not with attributes).