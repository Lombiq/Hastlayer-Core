﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Constants
{
    /// <summary>
    /// Stores an overview comment that will be inserted into the generated VHDL code to help understanding it.
    /// </summary>
    internal static class GeneratedCodeOverviewComment
    {
        // The strange formatting is so the output will be well formatted and e.g. have appropriate indentations.
        public const string Comment =
@"This IP was generated by Hastlayer from .NET code to mimic the original logic. Note the following:
* For each member (methods, functions) in .NET a state machine was generated. Each state machine's name corresponds to 
  the original member's name.
* Inputs and outputs are passed between state machines as shared objects.
* There are operations that take multiple clock cycles like interacting with the memory and long-running arithmetic operations 
  (modulo, division, multiplication). These are awaited in subsequent states but be aware that some states can take more 
  than one clock cycle to produce their output.
* The CallProxy process at the bottom dispatches executions to the state machine that was started from the outside.";
    }
}
