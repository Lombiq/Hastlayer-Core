using Hast.Catapult.Abstractions;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hast.Catapult
{
    public class CatapultXdcFileBuilder : XdcFileBuilder<CatapultDeviceManifest>
    {
        public override Task<XdcFile> BuildManifestAsync(
            IEnumerable<IArchitectureComponentResult> architectureComponentResults,
            Architecture hastIpArchitecture)
        {
            // Adding multi-cycle path constraints for Quartus.

            var anyMultiCycleOperations = false;
            var sdcExpression = new MultiCycleSdcStatementsAttributeExpression();

            foreach (var architectureComponentResult in architectureComponentResults)
            {
                foreach (var operation in architectureComponentResult.ArchitectureComponent.MultiCycleOperations)
                {
                    sdcExpression.AddPath(
                        // If the path is through a global signal (i.e. that doesn't have a parent process) then
                        // the parent should be empty.
                        operation.OperationResultReference.DataObjectKind == DataObjectKind.Variable ?
                            ProcessUtility.FindProcesses(new[] { architectureComponentResult.Body }).Single().Name :
                            string.Empty,
                        operation.OperationResultReference,
                        operation.RequiredClockCyclesCeiling);

                    anyMultiCycleOperations = true;
                }
            }

            if (anyMultiCycleOperations)
            {
                var alteraAttribute = new Attribute
                {
                    Name = "altera_attribute",
                    ValueType = KnownDataTypes.UnrangedString,
                };

                hastIpArchitecture.Declarations.Add(new LogicalBlock(
                    new LineComment(
                        "Adding multi-cycle path constraints for Quartus Prime. See: " +
                        "https://www.intel.com/content/www/us/en/programmable/support/support-resources/knowledge-base/solutions/rd05162013_635.html"),
                    alteraAttribute,
                    new AttributeSpecification
                    {
                        Attribute = alteraAttribute,
                        Of = hastIpArchitecture.ToReference(),
                        ItemClass = "architecture",
                        Expression = sdcExpression,
                    }));
            }

            return Task.FromResult<XdcFile>(null);
        }
    }
}
