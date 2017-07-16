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
                .WithScrubber(source =>
                {
                    source = Regex.Replace(source, @"-- Date and time:([0-9\-\s\:]*UTC)", "-- (Date and time removed for approval testing.)");
                    //-- Hast_IP ID: 70cda87fd8c73cdac2fd935ada6170a7e241daf53cba13e3dfe7c48c4e67e4ac
                    source = Regex.Replace(source, @"-- Hast_IP ID: ([0-9a-z]*)", "-- (Hast_IP ID removed for approval testing.)");

                    return source;
                });
        }
    }
}
