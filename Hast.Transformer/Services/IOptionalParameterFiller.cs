using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Arguments for optional method parameters can be omitted. This service fills those too so later processing these
    /// method calls can be easier. Note that method class include constructors as well.
    /// </summary>
    /// <example>
    /// The constructor's signature being:
    /// public BitMask(ushort size, bool allOne = false)
    /// 
    /// ...the following code:
    /// var resultFractionBits = new BitMask(left._environment.Size);
    /// 
    /// ...will be changed into:
    /// var resultFractionBits = new BitMask(left._environment.Size, false);
    /// </example>
    public interface IOptionalParameterFiller : IDependency
    {
        void FillOptionalParamters(SyntaxTree syntaxTree);
    }
}
