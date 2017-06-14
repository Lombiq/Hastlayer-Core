using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
{
    internal class ArraySizeHolder : IArraySizeHolder
    {
        private readonly Dictionary<string, IArraySize> _arraySizes = new Dictionary<string, IArraySize>();


        public IArraySize GetSize(AstNode arrayHolder)
        {
            IArraySize arraySize;
            _arraySizes.TryGetValue(arrayHolder.GetFullName(), out arraySize);
            return arraySize;
        }

        public void SetSize(AstNode arrayHolder, int length)
        {
            var holderName = arrayHolder.GetFullNameWithUnifiedPropertyName();

            IArraySize existingSize;
            if (_arraySizes.TryGetValue(holderName, out existingSize) && existingSize.Length != length)
            {
                throw new NotSupportedException(
                    "Array sizes should be statically defined but the array stored in the array holder \"" +
                    holderName + "\" has multiple length assigned (previously it had a length of " + existingSize.Length +
                    " and secondly a  length of " + length +
                    " specified). Make sure that a variable, field or property always stores an array of the same size (including target variables, fields and properties when it's passed around).");
            }

            _arraySizes[holderName] = new ArraySize { Length = length };
        }


        [DebuggerDisplay("{ToString()}")]
        private class ArraySize : IArraySize
        {
            public int Length { get; set; }


            public override string ToString() => "Length: " + Length.ToString();
        }
    }
}
