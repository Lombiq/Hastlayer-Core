using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Arguments for optional method parameters can be omitted. This service fills those too so later processing these
    /// method calls can be easier. Note that method class include constructors as well.
    /// </summary>
    /// <example>
    /// <para>
    /// The constructor's signature being:
    /// </para>
    /// <code>
    /// public BitMask(ushort size, bool allOne = false)
    /// </code>
    ///
    /// <para>...the following code:</para>
    /// <code>
    /// var resultFractionBits = new BitMask(left._environment.Size);
    /// </code>
    ///
    /// <para>...will be changed into:</para>
    /// <code>
    /// var resultFractionBits = new BitMask(left._environment.Size, false);
    /// </code>
    /// </example>
    public interface IOptionalParameterFiller : IDependency
    {
        void FillOptionalParamters(SyntaxTree syntaxTree);
    }
}
