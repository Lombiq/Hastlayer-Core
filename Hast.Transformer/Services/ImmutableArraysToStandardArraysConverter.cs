using System;
using System.Linq;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Hast.Transformer.Services
{
    public class ImmutableArraysToStandardArraysConverter : IImmutableArraysToStandardArraysConverter
    {
        public void ConvertImmutableArraysToStandardArrays(SyntaxTree syntaxTree)
        {
            // Note that ImmutableArrays can only be single-dimensional in themselves so the whole code here is built
            // around that assumption (though it's possible to create ImmutableArrays of ImmutableArrays, but this is
            // not supported yet).

            // This implementation is partial, to the extent needed for Unum support. More value holders, like fields,
            // could also be handled for example.

            syntaxTree.AcceptVisitor(new ImmutableArraysToStandardArraysConvertingVisitor());
        }


        private class ImmutableArraysToStandardArraysConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
            {
                base.VisitPropertyDeclaration(propertyDeclaration);

                var propertyDefinition = propertyDeclaration.Annotation<PropertyDefinition>();

                if (!IsImmutableArrayName(propertyDefinition.PropertyType.FullName)) return;

                // Re-wiring types to use a standard array instead.
                var arrayType = CreateArrayTypeFromImmutableArrayReference(propertyDefinition.PropertyType);
                propertyDefinition.PropertyType = arrayType;

                propertyDeclaration.ReturnType = CreateArrayAstTypeFromImmutableArrayAstType(propertyDeclaration.ReturnType, arrayType);
                propertyDeclaration.RemoveAnnotations<TypeReference>();
                propertyDeclaration.AddAnnotation(arrayType);

                ThrowIfMultipleMembersWithTheNameExist(propertyDeclaration);
            }

            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                // Handling the various ImmutableArray methods here.

                var memberReference = invocationExpression.Target as MemberReferenceExpression;

                var invocationTypeReference = invocationExpression.GetActualTypeReference();

                Func<MemberReferenceExpression> createArrayLengthExpression = () =>
                    new MemberReferenceExpression(memberReference.Target.Clone(), "Length")
                        .WithAnnotation(TypeHelper.CreateInt32TypeInformation());

                Func<Expression, MemberReferenceExpression, InvocationExpression> createArrayCopyExpression =
                    (destinationValueHolder, arrayLengthExpression) =>
                    // Just faking the name for InvocationExpressionTransformer.
                    new InvocationExpression(
                        new MemberReferenceExpression(new TypeReferenceExpression(new SimpleType("System.Array")), "Copy"),
                        memberReference.Target.Clone(),
                        destinationValueHolder.Clone(),
                        arrayLengthExpression.Clone())
                    .WithAnnotation(new DummyArrayCopyMemberDefinition());

                if (!IsImmutableArrayName(invocationTypeReference?.FullName) || memberReference == null)
                {
                    var methodReference = invocationExpression.Annotation<MethodReference>();

                    if (methodReference != null &&
                        IsImmutableArrayName(methodReference.DeclaringType.FullName) &&
                        methodReference.Name == "CopyTo")
                    {
                        invocationExpression.ReplaceWith(createArrayCopyExpression(
                            invocationExpression.Arguments.Single(),
                            createArrayLengthExpression()));
                    }
                }
                else
                {
                    var arrayTypeInformation = CreateArrayTypeInformationFromImmutableArrayReference(invocationTypeReference);

                    if (memberReference.MemberName == "CreateRange")
                    {
                        // ImmutableArray.CreateRange() can be substituted with an array assignment. Since the array
                        // won't be changed (because it's immutable in .NET) this is not dangerous.
                        if (invocationExpression.Arguments.Count > 1)
                        {
                            throw new NotSupportedException(
                                "Only the ImmutableArray.CreateRange() overload with a single argument is supported. The supplied expression was: " +
                                invocationExpression.ToString().AddParentEntityName(memberReference));
                        }

                        invocationExpression.ReplaceWith(invocationExpression.Arguments.Single().Clone());
                    }
                    else if (memberReference.MemberName == "SetItem")
                    {
                        // SetItem can be converted into a simple array element assignment to a newly created copy of the
                        // array.

                        var elementType = AstBuilder.ConvertType(((ArrayType)arrayTypeInformation.ExpectedType).ElementType);
                        var parentStatement = invocationExpression.FindFirstParentStatement();

                        var variableIdentifier = VariableHelper.DeclareAndReferenceArrayVariable(
                            memberReference.Target,
                            elementType,
                            arrayTypeInformation.ExpectedType);
                        var arrayLengthExpression = createArrayLengthExpression();

                        var arrayCreate = new ArrayCreateExpression();
                        arrayCreate.Arguments.Add(arrayLengthExpression);
                        arrayCreate.Type = elementType;
                        var arrayCreateAssignment = new AssignmentExpression(
                            variableIdentifier,
                            arrayCreate);
                        AstInsertionHelper.InsertStatementBefore(parentStatement, new ExpressionStatement(arrayCreateAssignment));

                        AstInsertionHelper.InsertStatementBefore(
                            parentStatement,
                            new ExpressionStatement(createArrayCopyExpression(variableIdentifier, arrayLengthExpression)));

                        var valueArgument = invocationExpression.Arguments.Skip(1).Single();

                        var assignment = new AssignmentExpression(
                            new IndexerExpression(variableIdentifier.Clone(), invocationExpression.Arguments.First().Clone()),
                            valueArgument.Clone());
                        assignment.AddAnnotation(valueArgument.Annotation<TypeInformation>());

                        AstInsertionHelper.InsertStatementBefore(parentStatement, new ExpressionStatement(assignment));

                        invocationExpression.ReplaceWith(variableIdentifier.Clone());
                    }
                    else if (memberReference.MemberName == "Create")
                    {
                        // ImmutableArray.Create() just creates an empty array.
                        var arrayCreate = new ArrayCreateExpression
                        {
                            Type = memberReference.TypeArguments.Single().Clone(),
                        };
                        var sizeExpression = new PrimitiveExpression(0)
                            .WithAnnotation(TypeHelper.CreateInt32TypeInformation());
                        arrayCreate.Arguments.Add(sizeExpression);
                        arrayCreate.AddAnnotation(arrayTypeInformation);
                        invocationExpression.ReplaceWith(arrayCreate);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "The member \"" + memberReference.MemberName +
                            "\" is not supported on ImmutableArray. The supplied expression was: " +
                            invocationExpression.ToString().AddParentEntityName(memberReference));
                    }
                }
            }

            public override void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
            {
                base.VisitParameterDeclaration(parameterDeclaration);

                var parameterDefinition = parameterDeclaration.Annotation<ParameterDefinition>();

                if (!IsImmutableArrayName(parameterDefinition.ParameterType.FullName)) return;

                // Re-wiring types to use a standard array instead.
                var arrayType = CreateArrayTypeFromImmutableArrayReference(parameterDefinition.ParameterType);
                parameterDefinition.ParameterType = arrayType;

                var arrayAstType = new ComposedType
                {
                    BaseType = ((SimpleType)parameterDeclaration.Type).TypeArguments.Single().Clone()
                };
                arrayAstType.ArraySpecifiers.Add(new ArraySpecifier(1));
                arrayAstType.AddAnnotation(arrayType);

                parameterDeclaration.Type = CreateArrayAstTypeFromImmutableArrayAstType(parameterDeclaration.Type, arrayType);

                ThrowIfMultipleMembersWithTheNameExist(parameterDeclaration.FindFirstParentEntityDeclaration());
            }

            public override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
            {
                base.VisitConditionalExpression(conditionalExpression);
                ChangeTypeInformationIfImmutableArrayReferencingExpression(conditionalExpression);
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                var typeReference = ChangeTypeInformationIfImmutableArrayReferencingExpression(identifierExpression);
                if (typeReference != null)
                {
                    identifierExpression.Annotation<ILVariable>().Type = typeReference;
                }
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);
                var typeReference = ChangeTypeInformationIfImmutableArrayReferencingExpression(memberReferenceExpression);
                if (typeReference != null)
                {
                    var methodReference = memberReferenceExpression.Annotation<MethodReference>();
                    if (methodReference != null) methodReference.ReturnType = typeReference;

                    var propertyReference = memberReferenceExpression.Annotation<PropertyReference>();
                    if (propertyReference != null) propertyReference.PropertyType = typeReference;
                }
            }

            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);
                ChangeTypeInformationIfImmutableArrayReferencingExpression(assignmentExpression);
            }

            public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
            {
                base.VisitVariableDeclarationStatement(variableDeclarationStatement);

                var type = variableDeclarationStatement.Type;
                if (IsImmutableArrayName(type.GetFullName()))
                {
                    var ilVariable = variableDeclarationStatement.Variables.Single().Annotation<ILVariable>();
                    ilVariable.Type = CreateArrayTypeFromImmutableArrayReference(ilVariable.Type);
                    type.ReplaceWith(CreateArrayAstTypeFromImmutableArrayAstType(type, (ArrayType)ilVariable.Type));
                }
            }


            private TypeReference ChangeTypeInformationIfImmutableArrayReferencingExpression(AstNode node)
            {
                var expressionTypeReference = node.GetActualTypeReference();

                if (!IsImmutableArrayName(expressionTypeReference?.FullName)) return null;

                node.RemoveAnnotations<TypeInformation>();
                var typeInformation = CreateArrayTypeInformationFromImmutableArrayReference(expressionTypeReference);
                node.AddAnnotation(typeInformation);

                return typeInformation.ExpectedType;
            }

            private static bool IsImmutableArrayName(string name) =>
                !string.IsNullOrEmpty(name) && name.StartsWith("System.Collections.Immutable.ImmutableArray");

            private static ArrayType CreateArrayTypeFromImmutableArrayReference(TypeReference typeReference) =>
                new ArrayType(((GenericInstanceType)typeReference).GenericArguments.Single(), 1);

            private static TypeInformation CreateArrayTypeInformationFromImmutableArrayReference(TypeReference typeReference)
            {
                return CreateArrayTypeFromImmutableArrayReference(typeReference).ToTypeInformation();
            }

            private static AstType GetClonedElementTypeFromImmutableArrayAstType(AstType astType) =>
                ((SimpleType)astType).TypeArguments.Single().Clone();

            private static ComposedType CreateArrayAstTypeFromImmutableArrayAstType(AstType astType, ArrayType arrayType)
            {
                var arrayAstType = new ComposedType
                {
                    BaseType = GetClonedElementTypeFromImmutableArrayAstType(astType)
                };
                arrayAstType.ArraySpecifiers.Add(new ArraySpecifier(1));
                arrayAstType.AddAnnotation(arrayType);
                return arrayAstType;
            }

            private static void ThrowIfMultipleMembersWithTheNameExist(EntityDeclaration member)
            {
                var fullName = member.GetFullName();
                if (member.FindFirstParentTypeDeclaration().Members.Count(m => m.GetFullName() == fullName) > 1)
                {
                    throw new NotSupportedException(
                        "ImmutableArrays are converted into standard arrays. After such conversions a new member with the signature " +
                        fullName +
                        " was created, tough a previously existing member has the same signature. Change the members so even after converting ImmutableArrays they will have unique signatures. The full declaration of the converted member: " +
                        Environment.NewLine + member.ToString());
                }
            }


            private class DummyArrayCopyMemberDefinition : IMemberDefinition
            {
                public Collection<CustomAttribute> CustomAttributes
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public TypeDefinition DeclaringType
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public string FullName => "System.Void System.Array::Copy(System.Array,System.Array,System.Int64)";

                public bool HasCustomAttributes
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public bool IsRuntimeSpecialName
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public bool IsSpecialName
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public MetadataToken MetadataToken
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public string Name
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
