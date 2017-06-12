using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
{
    // A separate model for this so adding support for multi-dimensional arrays will be possible by extending it.
    public interface IArraySize
    {
        int Length { get; }
    }

    /// <summary>
    /// Container for the sizes of statically sized arrays.
    /// </summary>
    public interface IArraySizeHolder
    {
        IArraySize GetSize(AstNode arrayHolder);
        void SetSize(AstNode arrayHolder, int length);
    }


    public static class ArraySizeHolderExtensions
    {
        public static IArraySize GetSizeOrThrow(this IArraySizeHolder arraySizeHolder, AstNode arrayHolder)
        {
            var size = arraySizeHolder.GetSize(arrayHolder);

            if (size == null)
            {
                throw new NotSupportedException(
                    "The length of the array holder " + arrayHolder.GetFullName() +
                    " couldn't be statically determined. Only arrays with dimensions defined at compile-time are supported.");
            }

            return size;
        }
    }
}
