using System.Linq;
using Hast.Transformer.Helpers;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    internal class ObjectHoldersToSubstitutedConstructorsMappingVisitor : DepthFirstAstVisitor
    {
        private readonly ConstantValuesSubstitutingAstProcessor _constantValuesSubstitutingAstProcessor;


        public ObjectHoldersToSubstitutedConstructorsMappingVisitor(ConstantValuesSubstitutingAstProcessor constantValuesSubstitutingAstProcessor)
        {
            _constantValuesSubstitutingAstProcessor = constantValuesSubstitutingAstProcessor;
        }


        public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        {
            base.VisitObjectCreateExpression(objectCreateExpression);

            // Substituting everything in the matching constructor (in its copy) and mapping that the object creation. 
            // So if the ctor sets some read-only members in a static way then the resulting object's members can get 
            // those substituted, without substituting them globally for all instances. This is important for 
            // bootstrapping substitution if there is a circular dependency between members and constructors (e.g.
            // a field's value set in the ctor depends on a ctor argument, which in turn depends on the same field of
            // another instance).

            if (objectCreateExpression.Parent.Is(assignment =>
                assignment.Left.GetActualTypeReference()?.FullName == objectCreateExpression.Type.GetFullName(),
                out AssignmentExpression parentAssignment))
            {
                var constructorName = objectCreateExpression.GetConstructorFullName();

                if (string.IsNullOrEmpty(constructorName)) return;

                var createdTypeName = objectCreateExpression.Type.GetFullName();

                var constructorType =
                    _constantValuesSubstitutingAstProcessor.TypeDeclarationLookupTable
                    .Lookup(createdTypeName);

                if (constructorType == null) ExceptionHelper.ThrowDeclarationNotFoundException(createdTypeName);

                var constructorDeclaration = constructorType
                    .Members
                    .SingleOrDefault(member => member.GetFullName() == constructorName);

                if (constructorDeclaration == null) return;

                var constructorDeclarationClone = (MethodDeclaration)constructorDeclaration.Clone();

                var subConstantValuesTable = _constantValuesSubstitutingAstProcessor.ConstantValuesTable.Clone();

                foreach (var argument in objectCreateExpression.Arguments.Where(argument => argument is PrimitiveExpression))
                {
                    var parameter = ConstantValueSubstitutionHelper.FindConstructorParameterForPassedExpression(
                        objectCreateExpression,
                        argument,
                        _constantValuesSubstitutingAstProcessor.TypeDeclarationLookupTable);

                    subConstantValuesTable.MarkAsPotentiallyConstant(parameter, (PrimitiveExpression)argument, constructorDeclarationClone);
                }

                new ConstantValuesSubstitutingAstProcessor(
                    subConstantValuesTable,
                    _constantValuesSubstitutingAstProcessor.TypeDeclarationLookupTable,
                    _constantValuesSubstitutingAstProcessor.ArraySizeHolder,
                    _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings,
                    _constantValuesSubstitutingAstProcessor.AstExpressionEvaluator)
                .SubstituteConstantValuesInSubTree(constructorDeclarationClone, true);

                _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings[parentAssignment.Left.GetFullName()] =
                    constructorDeclarationClone;

                // Also pass the object initialization data to the "this" reference. So if methods are called from the
                // constructor (or other objects created) then there the scope of the ctor will be accessible too.
                var thisReference = constructorDeclaration
                    .FindFirstChildOfType<IdentifierExpression>(identifier => identifier.Identifier == "this");
                if (thisReference != null)
                {
                    _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings[thisReference.GetFullName()] =
                        constructorDeclarationClone;
                }
            }
        }
    }
}
