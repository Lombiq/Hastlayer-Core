using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Configuration
{
    public class MemberInvokationInstanceCountConfiguration
    {
        /// <summary>
        /// Gets the prefix of the member's name. Use the same convention as with 
        /// <see cref="HardwareGenerationConfiguration.PublicHardwareMemberNamePrefixes"/>
        /// </summary>
        public string MemberNamePrefix { get; private set; }

        /// <summary>
        /// Gets or sets the maximal recursion depth of the member. When using (even indirectly) recursive invokations
        /// between members set the maximal depth here.
        /// </summary>
        /// <example>
        /// A value of 3 would mean that the member can invoke itself three times recursively, i.e. there is the
        /// member, then it calls itself (depth 1), then it calls itself (depth 2), then it calls itself (depth 3)
        /// before returning.
        /// </example>
        public ushort MaxRecursionDepth { get; set; }

        private ushort _maxDegreeOfParallelism;
        /// <summary>
        /// Gets or sets the maximal degree of parallelism that will be attempted to build into the generated hardware
        /// when constructs suitable for hardware-level parallelisation are found.
        /// </summary>
        /// <example>
        /// A value of 3 would mean that maximally 3 instances will be able to be executed in parallel.
        /// </example>
        public ushort MaxDegreeOfParallelism
        {
            get { return _maxDegreeOfParallelism; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("The max degree of parallelism should be at least 1, otherwise the member wouldn't be transformed at all.");
                }

                _maxDegreeOfParallelism = value;
            }
        }

        // But why do I need to cast to uint? http://stackoverflow.com/questions/10065287/why-is-ushort-ushort-equal-to-int#comment58098182_10157517
        public uint MaxInvokationInstanceCount { get { return (uint)(MaxRecursionDepth + MaxDegreeOfParallelism); } }


        public MemberInvokationInstanceCountConfiguration(string memberNamePrefix)
        {
            MemberNamePrefix = memberNamePrefix;
            MaxDegreeOfParallelism = 1;
        }
    }
}
