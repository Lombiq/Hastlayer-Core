using System.Linq;
using System.Text;
using Hast.VhdlBuilder.Representation;

namespace System.Collections.Generic
{
    public static class VhdlElementEnumerableExtensions
    {
        public static string ToVhdl<T>(
            this IEnumerable<T> elements,
            IVhdlGenerationOptions vhdlGenerationOptions)
            where T : IVhdlElement
        {
            return elements.ToVhdl(vhdlGenerationOptions, string.Empty, null);
        }

        public static string ToVhdl<T>(
            this IEnumerable<T> elements,
            IVhdlGenerationOptions vhdlGenerationOptions,
            string elementTerminator,
            string lastElementTerminator = null)
            where T : IVhdlElement
        {
            if (elements == null || !elements.Any()) return string.Empty;

            // It's efficient to run this parallelized implementation even with a low number of items (or even one)
            // because the overhead of checking whether there are more than a few elements is bigger than the below
            // ceremony.
            var elementsArray = elements.ToArray();
            var lastElement = elementsArray[elementsArray.Length - 1];
            var resultArray = new string[elementsArray.Length];

            Threading.Tasks.Parallel.For(0, elementsArray.Length - 1, i =>
            {
                resultArray[i] = elementsArray[i].ToVhdl(vhdlGenerationOptions) + elementTerminator;
            });

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < resultArray.Length; i++)
            {
                stringBuilder.Append(resultArray[i]);
            }

            if (lastElementTerminator == null) lastElementTerminator = elementTerminator;
            stringBuilder.Append(lastElement.ToVhdl(vhdlGenerationOptions) + lastElementTerminator);

            return stringBuilder.ToString();
        }
    }
}
