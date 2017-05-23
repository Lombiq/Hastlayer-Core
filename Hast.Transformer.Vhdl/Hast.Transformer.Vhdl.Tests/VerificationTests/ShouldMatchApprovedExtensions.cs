using System.Text.RegularExpressions;
using Shouldly;
using Shouldly.Configuration;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public static class ShouldMatchApprovedExtensions
    {
        /// <summary>
        /// Match the input VHDL source against an existing approved source file. This quickly tells if something changed.
        /// </summary>
        /// <remarks>
        /// Also see: http://shouldly.readthedocs.io/en/latest/assertions/shouldMatchApproved.html
        /// </remarks>
        /// <param name="vhdlSource"></param>
        public static void ShouldMatchApprovedWithVhdlConfiguration(this string vhdlSource)
        {
            vhdlSource.ShouldMatchApproved(configurationBuilder => 
                configurationBuilder.WithVhdlConfiguration().UseCallerLocation());
        }
    }


    public static class ShouldMatchConfigurationBuilderExtensions
    {
        public static ShouldMatchConfigurationBuilder WithVhdlConfiguration(this ShouldMatchConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder
                .SubFolder(System.IO.Path.Combine("VerificationSources"))
                .WithFileExtension("vhdl")
                .WithScrubber(source => Regex.Replace(source, @"-- Date and time:([0-9\.\s\:]*UTC)", "-- (Date and time removed for approval testing.)"));
        }
    }
}
