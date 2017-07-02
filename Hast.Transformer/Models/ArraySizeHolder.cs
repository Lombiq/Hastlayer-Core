﻿using System;
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
        private readonly Dictionary<string, IArraySize> _preConfiguredArraySizes;


        public ArraySizeHolder()
        {
        }

        public ArraySizeHolder(Dictionary<string, IArraySize> preConfiguredArraySizes)
        {
            _arraySizes = new Dictionary<string, IArraySize>(preConfiguredArraySizes);
            _preConfiguredArraySizes = new Dictionary<string, IArraySize>(preConfiguredArraySizes);
        }


        public IArraySize GetSize(AstNode arrayHolder)
        {
            IArraySize arraySize;
            _arraySizes.TryGetValue(arrayHolder.GetFullNameWithUnifiedPropertyName(), out arraySize);
            return arraySize;
        }

        public void SetSize(AstNode arrayHolder, int length)
        {
            var holderName = arrayHolder.GetFullNameWithUnifiedPropertyName();

            IArraySize existingSize;
            if (_arraySizes.TryGetValue(holderName, out existingSize))
            {
                if (existingSize.Length != length)
                {
                    if (_preConfiguredArraySizes.ContainsKey(holderName))
                    {
                        // If the size of the array was pre-configured then ignore the mismatch, the array will have the
                        // configured size, no matter what.
                        return;
                    }

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
    }
}
