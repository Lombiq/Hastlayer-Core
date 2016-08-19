using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public class ParameterSignal : Signal
    {
        public string TargetMemberFullName { get; private set; }
        public string TargetParameterName { get; private set; }
        public int Index { get; set; }

        /// <summary>
        /// Indicates whether the parameter is the own (in or out) parameter of the component (as opposed to being
        /// a parameter passed to invoked components).
        /// </summary>
        public bool IsOwn { get; set; }


        public ParameterSignal(string targetMemberFullName, string parameterName)
        {
            TargetMemberFullName = targetMemberFullName;
            TargetParameterName = parameterName;
        }
    }
}
