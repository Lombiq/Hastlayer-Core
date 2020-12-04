using System.Diagnostics.CodeAnalysis;

namespace Hast.TestInputs.Invalid
{
    [SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Sample.")]
    [SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "Sample.")]
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Sample.")]
    public class InvalidModel
    {
        // Static fields are not supported.
        public static int StaticField = 10;
    }
}
