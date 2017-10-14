﻿namespace Hast.Transformer.Vhdl.Constants
{
    /// <summary>
    /// Stores long comments that are inserted into the generated VHDL code to help understand it.
    /// </summary>
    internal static class LongGeneratedCodeComments
    {
        // The strange formatting is so the output will be well formatted and e.g. have appropriate indentations.
        public const string Libraries =
@"VHDL libraries necessary for the generated code to work. These libraries are included here instead of being managed separately in the Hardware Framework so they can be more easily updated.";

        /// <summary>
        /// Stores an overview comment that is inserted into the generated VHDL code to help understand it.
        /// </summary>
        public const string Overview =
@"This IP was generated by Hastlayer from .NET code to mimic the original logic. Note the following:
* For each member (methods, functions) in .NET a state machine was generated. Each state machine's name corresponds to 
  the original member's name.
* Inputs and outputs are passed between state machines as shared objects.
* There are operations that take multiple clock cycles like interacting with the memory and long-running arithmetic operations 
  (modulo, division, multiplication). These are awaited in subsequent states but be aware that some states can take more 
  than one clock cycle to produce their output.
* The ExternalInvocationProxy process dispatches invocations that were started from the outside to the state machines.
* The InternalInvocationProxy processes dispatch invocations between state machines.";
    }
}
