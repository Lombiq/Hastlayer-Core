using Hast.Transformer.Vhdl.SubTransformers;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Verifiers
{
    public class UnsupportedConstructsVerifier : IUnsupportedConstructsVerifier
    {
        private readonly IDisplayClassFieldTransformer _displayClassFieldTransformer;

        public UnsupportedConstructsVerifier(IDisplayClassFieldTransformer displayClassFieldTransformer) => _displayClassFieldTransformer = displayClassFieldTransformer;

        public void ThrowIfUnsupportedConstructsFound(SyntaxTree syntaxTree) => syntaxTree.AcceptVisitor(new UnsupportedConstructsFindingVisitor(_displayClassFieldTransformer));

        private class UnsupportedConstructsFindingVisitor : DepthFirstAstVisitor
        {
            private readonly IDisplayClassFieldTransformer _displayClassFieldTransformer;

            public UnsupportedConstructsFindingVisitor(IDisplayClassFieldTransformer displayClassFieldTransformer) => _displayClassFieldTransformer = displayClassFieldTransformer;

            public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
            {
                base.VisitFieldDeclaration(fieldDeclaration);

                if (fieldDeclaration.HasModifier(Modifiers.Static) &&
                    !_displayClassFieldTransformer.IsDisplayClassField(fieldDeclaration))
                {
                    throw new NotSupportedException(
                        fieldDeclaration.GetFullName() + " is a static field. " +
                        "Static fields are not supported, see: https://github.com/Lombiq/Hastlayer-SDK/issues/24."
                        .AddParentEntityName(fieldDeclaration));
                }
            }
        }
    }
}
