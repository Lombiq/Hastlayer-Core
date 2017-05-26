﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ArrayParameterLengthSetter : IArrayParameterLengthSetter
    {
        public void SetArrayParameterSizes(SyntaxTree syntaxTree)
        {
            var methodsWithArrayParameters = syntaxTree
                .GetAllTypeDeclarations()
                .SelectMany(typeDeclaration => typeDeclaration.Members)
                .Where(member => member is MethodDeclaration)
                .Cast<MethodDeclaration>()
                .Where(method => method.Parameters.Any(parameter => parameter.Type.IsArray()));

            if (!methodsWithArrayParameters.Any()) return;

            syntaxTree.AcceptVisitor(new ArrayParameterPassingFindingVisitor(methodsWithArrayParameters));
        }


        private class ArrayParameterPassingFindingVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, MethodDeclaration> _methodsWithArrayParameters;


            public ArrayParameterPassingFindingVisitor(IEnumerable<MethodDeclaration> methodsWithArrayParameters)
            {
                _methodsWithArrayParameters = methodsWithArrayParameters.ToDictionary(method => method.GetFullName());
            }


            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                if (invocationExpression.IsSimpleMemoryInvocation()) return;

                ProcessExpression(invocationExpression, invocationExpression.Arguments);
            }

            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);
                ProcessExpression(objectCreateExpression, objectCreateExpression.Arguments);
            }


            private void ProcessExpression(Expression expression, AstNodeCollection<Expression> arguments)
            {
                MethodDeclaration method;
                if (!_methodsWithArrayParameters.TryGetValue(expression.GetFullName(), out method)) return;

                var arrayArguments = arguments
                    .Where(argument => argument.GetActualTypeReference().IsArray)
                    .ToArray();
                var arrayParameters = method.Parameters
                    .Where(parameter => parameter.Type.IsArray())
                    .ToArray();

                for (int i = 0; i < arrayArguments.Length; i++)
                {
                    var arrayCreationLengthFindingVisitor = new ArrayCreationLengthFindingVisitor(arrayArguments[i].GetFullName());

                    expression.FindFirstParentEntityDeclaration().AcceptVisitor(arrayCreationLengthFindingVisitor);

                    var existingParameterArrayLength = arrayParameters[i].Annotation<ParameterArrayLength>();
                    if (existingParameterArrayLength == null)
                    {
                        arrayParameters[i].AddAnnotation(new ParameterArrayLength(arrayCreationLengthFindingVisitor.Length));
                    }
                    else if (existingParameterArrayLength.Length != arrayCreationLengthFindingVisitor.Length)
                    {
                        throw new InvalidOperationException(
                            "Array sizes should be statically defined but the array parameter \"" +
                            arrayParameters[i].GetFullName() +
                            "\" was assigned to from multiple differently sized sources (the firstly assigned array had the length " +
                            existingParameterArrayLength.Length + ", the secondly assigned " +
                            arrayCreationLengthFindingVisitor.Length +
                            "). Make sure that all arrays passed to this parameter are of the same size.");
                    }
                }
            }
        }

        private class ArrayCreationLengthFindingVisitor : DepthFirstAstVisitor
        {
            private readonly string _variableFullName;

            public int Length { get; private set; }


            public ArrayCreationLengthFindingVisitor(string variableFullName)
            {
                _variableFullName = variableFullName;
            }


            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                if (arrayCreateExpression.FindFirstParentOfType<AssignmentExpression>().Left.GetFullName() != _variableFullName)
                {
                    return;
                }

                Length = arrayCreateExpression.GetStaticLength();
            }
        }
    }
}
