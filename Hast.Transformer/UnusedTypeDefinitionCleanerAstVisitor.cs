using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer
{
    class UnusedTypeDefinitionCleanerAstVisitor : DepthFirstAstVisitor
    {
        public override void VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression)
        {
            base.VisitTypeReferenceExpression(typeReferenceExpression);
        }
    }
}
