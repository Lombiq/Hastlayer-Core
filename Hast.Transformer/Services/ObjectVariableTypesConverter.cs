﻿using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class ObjectVariableTypesConverter : IObjectVariableTypesConverter
    {
        public void ConvertObjectVariableTypes(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new MethodObjectParametersTypeConvertingVisitor());
        }


        private class MethodObjectParametersTypeConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                base.VisitMethodDeclaration(methodDeclaration);

                foreach (var objectParameter in methodDeclaration.Parameters
                    .Where(parameter => parameter.Type.Is<PrimitiveType>(type =>
                        type.KnownTypeCode == KnownTypeCode.Object)))
                {
                    var castExpressionFindingVisitor = new ParameterCastExpressionFindingVisitor(objectParameter.Name);
                    methodDeclaration.Body.AcceptVisitor(castExpressionFindingVisitor);

                    // Simply changing the parameter's type and removing the cast. Note that this will leave corresponding
                    // compiler-generated Funcs intact and thus wrong. E.g. there will be similar lines added to the
                    // lambda's calling method:
                    // Func<object, bool> arg_57_1;
                    // if (arg_57_1 = PrimeCalculator.<> c.<> 9__9_0 == null) {
                    //     arg_57_1 = PrimeCalculator.<> c.<> 9__9_0 = new Func<object, bool>(PrimeCalculator.<> c.<> 9.< ParallelizedArePrimeNumbers > b__9_0);
                    // }
                    // This will remain, despite the Func's type now correctly being e.g. Func<uint, bool>.

                    var castExpression = castExpressionFindingVisitor.Expression;
                    if (castExpression != null)
                    {
                        var actualType = castExpression.GetActualType();
                        objectParameter.Type = castExpression.Type.Clone();
                        objectParameter.RemoveAnnotations(typeof(ILVariableResolveResult));
                        objectParameter.AddAnnotation(VariableHelper
                            .CreateILVariableResolveResult(
                                ICSharpCode.Decompiler.IL.VariableKind.Parameter,
                                castExpression.Type.GetActualType(),
                                objectParameter.Name));
                        castExpression.ReplaceWith(castExpression.Expression);
                        castExpression.Remove();

                        methodDeclaration.Body.AcceptVisitor(new ParameterReferencesTypeChangingVisitor(objectParameter.Name, actualType));
                    }
                }
            }


            private class ParameterCastExpressionFindingVisitor : DepthFirstAstVisitor
            {
                private readonly string _parameterName;

                public CastExpression Expression { get; private set; }


                public ParameterCastExpressionFindingVisitor(string parameterName)
                {
                    _parameterName = parameterName;
                }


                public override void VisitCastExpression(CastExpression castExpression)
                {
                    base.VisitCastExpression(castExpression);

                    if (castExpression.Expression.Is<IdentifierExpression>(identifier => identifier.Identifier == _parameterName))
                    {
                        // If there are multiple casts for the given parameter then we'll deal with it as an object unless
                        // all casts are for the same type.
                        if (Expression == null || Expression.Type == castExpression.Type)
                        {
                            Expression = castExpression;
                        }
                        else
                        {
                            Expression = null;
                        }
                    }
                }
            }

            private class ParameterReferencesTypeChangingVisitor : DepthFirstAstVisitor
            {
                private readonly string _parameterName;
                private readonly IType _type;


                public ParameterReferencesTypeChangingVisitor(string parameterName, IType type)
                {
                    _parameterName = parameterName;
                    _type = type;
                }


                public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
                {
                    base.VisitIdentifierExpression(identifierExpression);

                    if (identifierExpression.Identifier != _parameterName) return;

                    identifierExpression.ReplaceAnnotations(_type.ToResolveResult());
                }
            }
        }
    }
}
