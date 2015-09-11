using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Function : ISubProgram
    {
        public string Name { get; set; }
        public List<FunctionArgument> Arguments { get; set; }
        public DataType ReturnType { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Function()
        {
            Arguments = new List<FunctionArgument>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            var subContext = vhdlGenerationContext.CreateContextForSubLevel();

            return Terminated.Terminate(
                "function " + Name + 
                " (" + string.Join("; ", Arguments.Select(parameter => parameter.ToVhdl(vhdlGenerationContext))) + ")" + 
                " return " + ReturnType.Name + " is " + vhdlGenerationContext.NewLineIfShouldFormat() +
                    Declarations.ToVhdl(subContext) + (Declarations != null && Declarations.Any() ? " " : string.Empty) +
                "begin " + vhdlGenerationContext.NewLineIfShouldFormat() +
                    Body.ToVhdl(subContext) +
                " end " + Name, vhdlGenerationContext);
        }
    }


    public class FunctionArgument : TypedDataObjectBase
    {
        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                (DataObjectKind.ToString() ?? string.Empty) +
                Name +
                ": " +
                DataType.ToVhdl(vhdlGenerationContext);
        }
    }
}
