using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Visitors
{
    public class InstanceMethodsToStaticConvertingVisitor : DepthFirstAstVisitor
    {
        public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        {
            base.VisitMethodDeclaration(methodDeclaration);

            // We only have to deal with instance methods of non-interface classes.
            if (methodDeclaration.HasModifier(Modifiers.Static) || 
                methodDeclaration.IsInterfaceMember() ||
                methodDeclaration.FindFirstParentTypeDeclaration().Members.Any(member => member.IsInterfaceMember()))
            {
                return;
            }


            var thisArgument = new ParameterDeclaration("this", ParameterModifier.This);
            if (!methodDeclaration.Parameters.Any())
            {
                methodDeclaration.Parameters.Add(thisArgument); 
            }
            else
            {
                methodDeclaration.Parameters.InsertBefore(methodDeclaration.Parameters.First(), thisArgument);
            }
        }
    }
}
