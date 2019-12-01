using ICSharpCode.Decompiler.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// If a newly created object's members are accessed directly instead of assigning the object to a variable for 
    /// example, then this service will add an intermediary variable for easier later processing.
    /// </summary>
    /// <example>
    /// var size = new BitMask(Size).Size;
    /// 
    /// ...will be converted into the following form:
    /// var bitMask = new BitMask(Size);
    /// var size = bitMask.Size;
    /// </example>
    public interface IDirectlyAccessedNewObjectVariablesCreator : IDependency
    {
        void CreateVariablesForDirectlyAccessedNewObjects(SyntaxTree syntaxTree);
    }
}
