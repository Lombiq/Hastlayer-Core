using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Processes binary and unary operator expressions and if the operands or the result lacks necessary explicit 
    /// casts, adds them.
    /// 
    /// Arithmetic binary operations in .NET should have set operand and result types, but these are not alway reflected
    /// with explicit casts in the AST. The rules for determining the type of operands are similar to that in C/C++ 
    /// (<see href="https://docs.microsoft.com/en-us/cpp/c-language/usual-arithmetic-conversions"/>) and are called 
    /// "numeric promotions" <see href="https://github.com/dotnet/csharplang/blob/master/spec/expressions.md#numeric-promotions"/>).
    /// But the AST won't always contain all casts due to implicit casting. Take the following code for example:
    /// 
    /// byte a = ...;
    /// byte b = ...;
    /// var x = (short)(a + b)
    /// 
    /// This explicitly cast byte operation will contain implicit casts to the operands like this:
    /// 
    /// var x = (short)((int)a + (int)b)
    /// 
    /// But only the top code will be in the AST. This service adds explicit casts for all implicit ones for easier
    /// processing later.
    /// 
    /// You can read about the background of why .NET works this way here: 
    /// https://stackoverflow.com/questions/941584/byte-byte-int-why.
    /// </summary>
    public interface IBinaryAndUnaryOperatorExpressionsCastAdjuster : IDependency
    {
        void AdjustBinaryAndUnaryOperatorExpressions(SyntaxTree syntaxTree, IKnownTypeLookupTable knownTypeLookupTable);
    }
}
