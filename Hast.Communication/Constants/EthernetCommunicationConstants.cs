using Orchard.Environment.Extensions;

namespace Hast.Communication.Constants
{
    [OrchardFeature("Hast.Communication.Ethernet")]
    internal static class EthernetCommunicationConstants
    {
        public static class Signals
        {
            public const char Busy = 'b';
            public const char Ready = 'r';
        }
    }
}
