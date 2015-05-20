# Hastlayer - VHDL Readme



Component containing implementations for transforming .NET assemblies into VHDL hardware description.


## Simple memory access

Currently to support dynamic memory allocation a very simple memory model is used (called Simple Memory): the transformable code has access to a fixed-size (size determined in runtime, per method calls) memory space that is represented in C# as a byte array (and implemented in the `SimpleMemory` class). All input and output values should be stored in this object, as well as any on the fly memory allocations can only happen through this class.

The .NET code can continue to use primitive types as normally.

In VHDL SimpleMemory method arguments are swapped with signal argument that ultimately let procedures access the Simle Memory-related ports. This is needed because procedure's can't directly access entity ports, not even when declared in the same architecture.