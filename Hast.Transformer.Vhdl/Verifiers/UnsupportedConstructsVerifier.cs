using ICSharpCode.NRefactory.CSharp;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Verifiers
{
    public class UnsupportedConstructsVerifier : IUnsupportedConstructsVerifier
    {
        public void ThrowIfUnsupportedConstructsFound(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new UnsupportedConstructsFindingVisitor());
        }


        private class UnsupportedConstructsFindingVisitor : DepthFirstAstVisitor
        {
            public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
            {
                base.VisitFieldDeclaration(fieldDeclaration);

                if (fieldDeclaration.HasModifier(Modifiers.Static))
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