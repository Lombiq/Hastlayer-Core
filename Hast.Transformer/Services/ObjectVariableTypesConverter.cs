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
            syntaxTree.AcceptVisitor(new UnnecessaryObjectCastsRemovingVisitor());
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

                    // Simply changing the parameter's type and removing the cast. Note that this will leave
                    // corresponding compiler-generated Funcs intact and thus wrong. E.g. there will be similar lines
                    // added to the lambda's calling method:
                    // Task.Factory.StartNew ((Func<object, bool>)this.<ParallelizedArePrimeNumbers>b__9_0, num4)
                    // This will remain, despite the Func's type now correctly being e.g. Func<uint, bool>.
                    // Note that the method's full name will contain the original object parameter since the 
                    // MemberResolveResult will still the have original parameters. This will be an aesthetic issue 
                    // only though: Nothing else depends on the parameters being correct here. If we'd change these
                    // then the whole MemberResolveResult would need to be recreated (since parameter types, as well as
                    // the list of parameters is read-only), not just here but in all the references to this method too.

                    var castExpression = castExpressionFindingVisitor.Expression;
                    if (castExpression != null)
                    {
                        var actualType = castExpression.GetActualType();

                        objectParameter.Type = castExpression.Type.Clone();
                        objectParameter.RemoveAnnotations(typeof(ILVariableResolveResult));
                        objectParameter.AddAnnotation(VariableHelper
                            .CreateILVariableResolveResult(
                                ICSharpCode.Decompiler.IL.VariableKind.Parameter,
                                actualType,
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

        private class UnnecessaryObjectCastsRemovingVisitor : DepthFirstAstVisitor
        {
            public override void VisitCastExpression(CastExpression castExpression)
            {
                base.VisitCastExpression(castExpression);

                if (castExpression.GetActualType().FullName != typeof(object).FullName ||
                    !castExpression.Parent.Is<InvocationExpression>(invocation => invocation.IsTaskStart()))
                {
                    return;
                }

                castExpression.ReplaceWith(castExpression.Expression.Detach());
            }
        }
    }
}
