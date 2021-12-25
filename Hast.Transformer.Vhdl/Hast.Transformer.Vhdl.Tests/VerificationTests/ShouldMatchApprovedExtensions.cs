using Hast.Common.Models;
using Shouldly;
using Shouldly.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public static class ShouldMatchApprovedExtensions
    {
        /// <summary>
        /// Match the input VHDL hardware description's source, including the XDC constraints if available, against an
        /// existing approved source file. This quickly tells if something changed.
        /// </summary>
        /// <remarks>
        /// <para>Also see: <c>http://shouldly.readthedocs.io/en/latest/assertions/shouldMatchApproved.html</c>.</para>
        /// </remarks>
        public static void ShouldMatchApprovedWithVhdlConfiguration(
            this VhdlHardwareDescription hardwareDescription,
            string deviceName = null) =>
            (hardwareDescription.VhdlSource + hardwareDescription.XdcSource)
                .ShouldMatchApproved(configurationBuilder =>
                {
                    var configuration = configurationBuilder.WithVhdlConfiguration().UseCallerLocation();
                    if (!string.IsNullOrEmpty(deviceName)) configuration.WithDiscriminator(deviceName);
                });

        /// <summary>
        /// Match the input VHDL hardware descriptions' sources, including the XDC constraints if available, against an
        /// existing approved source file. This quickly tells if something changed.
        /// </summary>
        /// <remarks>
        /// <para>Also see: <c>http://shouldly.readthedocs.io/en/latest/assertions/shouldMatchApproved.html</c>.</para>
        /// </remarks>
        public static void ShouldMatchApprovedWithVhdlConfiguration(
            this IEnumerable<VhdlHardwareDescription> hardwareDescriptions,
            string deviceName = null) =>
            string.Join(
                string.Empty,
                hardwareDescriptions.Select(hardwareDescription => hardwareDescription.VhdlSource + hardwareDescription.XdcSource))
                .ShouldMatchApproved(configurationBuilder =>
                {
                    var configuration = configurationBuilder.WithVhdlConfiguration().UseCallerLocation();
                    if (!string.IsNullOrEmpty(deviceName)) configuration.WithDiscriminator(deviceName);
                });

        /// <summary>
        /// Match the input VHDL source against an existing approved source file. This quickly tells if something changed.
        /// </summary>
        /// <remarks>
        /// <para>Note that the two methods here can't be DRY because even with UseCallerLocation() Shouldly would loose
        /// track of where the verification file is.</para>
        /// </remarks>
        public static void ShouldMatchApprovedWithVhdlConfiguration(this string vhdlSource) =>
            vhdlSource.ShouldMatchApproved(configurationBuilder =>
                configurationBuilder.WithVhdlConfiguration().UseCallerLocation());
    }

    public static class ShouldMatchConfigurationBuilderExtensions
    {
        public static ShouldMatchConfigurationBuilder WithVhdlConfiguration(this ShouldMatchConfigurationBuilder configurationBuilder)
        {
            var builder = configurationBuilder
                .SubFolder(Path.Combine("VerificationSources"))
                .WithFileExtension("vhdl")
                .WithScrubber(source =>
                {
                    source = Regex.Replace(
                        source,
                        @"-- Generated by Hastlayer \(hastlayer.com\) at ([0-9\-\s\:]*UTC)",
                        "-- Generated by Hastlayer (hastlayer.com) at <date and time removed for approval testing>");
                    source = Regex.Replace(source, @"-- Date and time:([0-9\-\s\:]*UTC)", "-- (Date and time removed for approval testing.)");
                    source = Regex.Replace(source, @"-- Hast_IP ID: ([0-9a-z]*)", "-- (Hast_IP ID removed for approval testing.)");

                    return source;
                });

            // This is what the builder sets with no explicit WithFilenameGenerator. We decorate it using the
            // WithFilenameGenerator call below.
            var defaultFileNameGenerator = builder.Build().FilenameGenerator;

            // Alter the FileNameGenerator to strip out invalid path characters. Prevents weird file names like this:
            // StaticTestInputAssembliesVerificationTests.<ClassStructureAssembliesMatchApproved.received.vhdl
            builder = builder
                .WithFilenameGenerator((testMethodInfo, discriminator, type, extension) =>
                    string.Concat(
                        defaultFileNameGenerator(testMethodInfo, discriminator, type, extension)
                            .Split(Path.GetInvalidFileNameChars())));

            return builder;
        }
    }
}
