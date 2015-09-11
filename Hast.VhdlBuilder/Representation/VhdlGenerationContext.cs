using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation
{
    public interface IVhdlGenerationContext : IVhdlGenerationOptions
    {
        int IndentationLevel { get; }   
    }


    public class VhdlGenerationContext : VhdlGenerationOptions, IVhdlGenerationContext
    {
        public int IndentationLevel { get; set; }


        public VhdlGenerationContext(IVhdlGenerationContext vhdlGenerationContext)
            : this((IVhdlGenerationOptions)vhdlGenerationContext)
        {
            IndentationLevel = vhdlGenerationContext.IndentationLevel;
        }

        public VhdlGenerationContext(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            FormatCode = vhdlGenerationOptions.FormatCode;
            UseShortNames = vhdlGenerationOptions.UseShortNames;
        }

        public VhdlGenerationContext()
        {
        }

    }


    public static class VhdlGenerationContextExtensions
    {
        public static string NewLineIfShouldFormat(this IVhdlGenerationContext vhdlGenerationContext)
        {
            return vhdlGenerationContext.FormatCode ? Environment.NewLine : string.Empty;
        }

        public static string IndentIfShouldFormat(this IVhdlGenerationContext vhdlGenerationContext)
        {
            // Using spaces instead of tabs.
            return vhdlGenerationContext.FormatCode ? 
                string.Empty.PadLeft(vhdlGenerationContext.IndentationLevel * 4, ' ') : 
                string.Empty;
        }

        public static IVhdlGenerationContext CreateContextForSubLevel(this IVhdlGenerationContext currentLevelContext)
        {
            return new VhdlGenerationContext(currentLevelContext) { IndentationLevel = currentLevelContext.IndentationLevel + 1 };
        }
    }
}
