using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public interface IArchitectureComponent
    {
        /// <summary>
        /// Name of the component. This is a standard name, not a VHDL identifier.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Variables local to the component.
        /// </summary>
        IList<Variable> LocalVariables { get; }

        /// <summary>
        /// Variables corresponding to the component that are in the global namespace.
        /// </summary>
        IList<Variable> GlobalVariables { get; }

        /// <summary>
        /// Global signals declared for the component that are driven from inside the component.
        /// </summary>
        IList<Signal> InternallyDrivenSignals { get; }

        /// <summary>
        /// Global signals declared for the component that are driven from outside the component.
        /// </summary>
        IList<Signal> ExternallyDrivenSignals { get; }

        /// <summary>
        /// Track which other members are called from this component and in how many instances at a given time. I.e.
        /// if this FSM starts another FSM (which was originally e.g. a method call) then it will be visible here. If
        /// parallelization happens then the call instance count will be greater than 1 (i.e. the other member is called
        /// in more than one instance at a given time).
        /// </summary>
        IDictionary<string, int> OtherMemberMaxInvokationInstanceCounts { get; }

        /// <summary>
        /// Produces the declarations corresponding to the component that should be inserted into the head of the
        /// architecture element.
        /// </summary>
        IVhdlElement BuildDeclarations();

        /// <summary>
        /// Produces the body of the componentthat should be inserted into the body of the architecture element.
        /// </summary>
        IVhdlElement BuildBody();
    }
}
