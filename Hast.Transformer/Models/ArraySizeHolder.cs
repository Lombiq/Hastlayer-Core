using Hast.Common.Validation;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Hast.Transformer.Models
{
    internal class ArraySizeHolder : IArraySizeHolder
    {
        private readonly Dictionary<string, IArraySize> _arraySizes = new Dictionary<string, IArraySize>();


        public ArraySizeHolder()
        {
        }

        public ArraySizeHolder(Dictionary<string, IArraySize> preConfiguredArraySizes)
        {
            _arraySizes = new Dictionary<string, IArraySize>(preConfiguredArraySizes);
        }


        public IArraySize GetSize(AstNode arrayHolder)
        {
            _arraySizes.TryGetValue(arrayHolder.GetFullName(), out var arraySize);
            return arraySize;
        }

        public void SetSize(AstNode arrayHolder, int length)
        {
            Argument.ThrowIfNull(arrayHolder, nameof(arrayHolder));

            var holderName = arrayHolder.GetFullName();

            if (_arraySizes.TryGetValue(holderName, out var existingSize))
            {
                if (existingSize.Length != length)
                {
                    throw new NotSupportedException(
                        "Array sizes should be statically defined but the array stored in the array holder \"" +
                        holderName + "\" has multiple length assigned (previously it had a length of " + existingSize.Length +
                        " and secondly a  length of " + length +
                        " specified). Make sure that a variable, field or property always stores an array of the same size (including target variables, fields and properties when it's passed around).");
                }

                return;
            }

            _arraySizes[holderName] = new ArraySize { Length = length };
        }

        public IArraySizeHolder Clone() => new ArraySizeHolder(_arraySizes);
    }
}
