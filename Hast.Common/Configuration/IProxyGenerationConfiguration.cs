using System.Collections.Generic;

namespace Hast.Layer
{
    public interface IProxyGenerationConfiguration
    {
        /// <summary>
        /// Gets a dictionary that can contain settings for non-default configuration options.
        /// </summary>
        IDictionary<string, object> CustomConfiguration { get; }

        /// <summary>
        /// Gets the communication channel used for communicating with the FPGA (eg. Ethernet).
        /// </summary>
        string CommunicationChannelName { get; }

        /// <summary>
        /// Gets or whether the results coming from the hardware implementation should be validated against a software 
        /// execution. If set to <c>true</c> then both a hardware and software invocation will happen and the result of 
        /// the two compared. If there is a mismatch then an exception will be thrown.
        /// </summary>
        bool ValidateHardwareResults { get; }
    }
}
