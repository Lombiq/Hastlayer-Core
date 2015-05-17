﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard;
using Hast.Transformer.Vhdl.Models;
using Mono.Cecil;
using ICSharpCode.Decompiler.Ast;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Models;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IExpressionTransformer : IDependency
    {
        IVhdlElement Transform(Expression expression, ISubTransformerContext context, IBlockElement block);
    }


    public class ExpressionTransformer : IExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;


        public ExpressionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public IVhdlElement Transform(Expression expression, ISubTransformerContext context, IBlockElement block)
        {
            if (expression is AssignmentExpression)
            {
                var assignment = (AssignmentExpression)expression;
                return new Assignment
                {
                    AssignTo = (IDataObject)Transform(assignment.Left, context, block),
                    Expression = Transform(assignment.Right, context, block)
                };
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = (IdentifierExpression)expression;
                var reference = new DataObjectReference { DataObjectKind = DataObjectKind.Variable, Name = identifier.Identifier.ToExtendedVhdlId() };

                if (!(identifier.Parent is BinaryOperatorExpression)) return reference;

                // In VHDL the operands of binary operations should have the same type, so we need to do a type conversion if necessary.

                var binaryOperatorExpression = (BinaryOperatorExpression)identifier.Parent;
                var leftType = _typeConverter.ConvertTypeReference(binaryOperatorExpression.Left.GetActualType());
                var rightType = _typeConverter.ConvertTypeReference(binaryOperatorExpression.Right.GetActualType());

                if (leftType == rightType) return reference;

                var isLeft = binaryOperatorExpression.Left == expression;
                var thisType = isLeft ? leftType : rightType;
                var otherType = isLeft ? rightType : leftType;

                // We need to convert types in a way to keep precision. E.g. conerting an int to real is fine, but vica versa would cause
                // information loss.
                if ((thisType == KnownDataTypes.UnrangedInt || thisType == KnownDataTypes.Natural) && otherType == KnownDataTypes.Real)
                {
                    return ImplementTypeConversion(thisType, otherType, reference);
                }
                else
                {
                    throw new NotImplementedException("Converting from " + thisType.Name + " to " + otherType.Name + " to fix a binary expression is not supported as it could cause information loss due to rounding.");
                }
            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = (PrimitiveExpression)expression;

                var type = _typeConverter.ConvertTypeReference(expression.GetActualType());
                var valueString = primitive.Value.ToString();
                if (type.TypeCategory == DataTypeCategory.Numeric) valueString = valueString.Replace(',', '.'); // Replacing decimal comma to decimal dot.

                // If a constant value of type real doesn't contain a decimal separator then it will be detected as integer and a type conversion would
                // be needed. Thus we add a .0 to the end to indicate it's a real.
                if (type == KnownDataTypes.Real && !valueString.Contains('.'))
                {
                    valueString += ".0";
                }

                return new Value { DataType = type, Content = valueString };
            }
            else if (expression is BinaryOperatorExpression) return TransformBinaryOperatorExpression((BinaryOperatorExpression)expression, context, block);
            else if (expression is InvocationExpression) return TransformInvocationExpression((InvocationExpression)expression, context, block);
            // These are not needed at the moment. MemberReferenceExpression is handled in TransformInvocationExpression and a
            // ThisReferenceExpression can only happen if "this" is passed to a method, which is not supported.
            //else if (expression is MemberReferenceExpression)
            //{
            //    var memberReference = (MemberReferenceExpression)expression;
            //    return Transform(memberReference.Target, context, block) + "." + memberReference.MemberName;
            //}
            //else if (expression is ThisReferenceExpression)
            //{
            //    var thisRef = expression as ThisReferenceExpression;
            //    return context.Scope.Method.Parent.GetFullName();
            //}
            else if (expression is UnaryOperatorExpression)
            {
                var unary = expression as UnaryOperatorExpression;
                return new Invokation
                {
                    Target = new Value { DataType = KnownDataTypes.Identifier, Content = "not" },
                    Parameters = new List<IVhdlElement> { Transform(unary.Expression, context, block) }
                };
            }
            else if (expression is TypeReferenceExpression)
            {
                var type = ((TypeReferenceExpression)expression).Type;
                var declaration = context.TransformationContext.TypeDeclarationLookupTable.Lookup(type);

                if (declaration == null)
                {
                    throw new InvalidOperationException("No matching type for \"" + ((SimpleType)type).Identifier + "\" found in the syntax tree. This can mean that the type's assembly was not added to the syntax tree.");
                }

                return new Value { DataType = KnownDataTypes.Identifier, Content = declaration.GetFullName() };
            }
            else if (expression is CastExpression)
            {
                return TransformCastExpression((CastExpression)expression, context, block);
            }
            else throw new NotSupportedException("Expressions of type " + expression.GetType() + " are not supported.");
        }


        private IVhdlElement TransformBinaryOperatorExpression(BinaryOperatorExpression expression, ISubTransformerContext context, IBlockElement block)
        {
            var binary = new Binary
            {
                Left = Transform(expression.Left, context, block),
                Right = Transform(expression.Right, context, block)
            };

            //  Would need to decide between + and & or sll/srl and sra/sla
            // See: http://www.csee.umbc.edu/portal/help/VHDL/operator.html
            switch (expression.Operator)
            {
                case BinaryOperatorType.Add:
                    binary.Operator = "+";
                    break;
                case BinaryOperatorType.Any:
                    break;
                case BinaryOperatorType.BitwiseAnd:
                    break;
                case BinaryOperatorType.BitwiseOr:
                    break;
                case BinaryOperatorType.ConditionalAnd:
                    break;
                case BinaryOperatorType.ConditionalOr:
                    break;
                case BinaryOperatorType.Divide:
                    binary.Operator = "/";
                    break;
                case BinaryOperatorType.Equality:
                    binary.Operator = "=";
                    break;
                case BinaryOperatorType.ExclusiveOr:
                    binary.Operator = "XOR";
                    break;
                case BinaryOperatorType.GreaterThan:
                    binary.Operator = ">";
                    break;
                case BinaryOperatorType.GreaterThanOrEqual:
                    binary.Operator = ">=";
                    break;
                case BinaryOperatorType.InEquality:
                    binary.Operator = "/=";
                    break;
                case BinaryOperatorType.LessThan:
                    binary.Operator = "<";
                    break;
                case BinaryOperatorType.LessThanOrEqual:
                    binary.Operator = "<=";
                    break;
                case BinaryOperatorType.Modulus:
                    binary.Operator = "mod";
                    break;
                case BinaryOperatorType.Multiply:
                    binary.Operator = "*";
                    break;
                case BinaryOperatorType.NullCoalescing:
                    break;
                case BinaryOperatorType.ShiftLeft:
                    binary.Operator = "sll";
                    break;
                case BinaryOperatorType.ShiftRight:
                    binary.Operator = "srl";
                    break;
                case BinaryOperatorType.Subtract:
                    binary.Operator = "-";
                    break;
            }

            return binary;
        }

        private IVhdlElement TransformInvocationExpression(InvocationExpression expression, ISubTransformerContext context, IBlockElement block)
        {
            var targetMemberReference = expression.Target as MemberReferenceExpression;
            var transformedParameters = new List<IVhdlElement>(expression.Arguments.Select(argument =>
                {
                    // Procedures won't accept constants and variables as parameters simultaneously so we have to copy constants to a variable
                    // and then use the variable as a parameter.
                    if (argument is PrimitiveExpression)
                    {
                        var primitiveArgument = ((PrimitiveExpression)argument);
                        var type = _typeConverter.ConvertTypeReference(primitiveArgument.GetActualType());

                        var variable = new Variable
                        {
                            Name = GetNextUnusedTemporalVariableName(expression.ToString(), "argument", context),
                            DataType = type
                        };

                        context.Scope.SubProgram.Declarations.Add(variable);

                        block.Body.Add(new Terminated(new Assignment
                        {
                            AssignTo = variable.ToReference(),
                            Expression = new Value { DataType = type, Content = primitiveArgument.Value.ToString() }
                        }));

                        return variable.ToReference();
                    }

                    return Transform(argument, context, block);
                }));


            if (context.TransformationContext.UseSimpleMemory() &&
                targetMemberReference != null &&
                targetMemberReference.Target is IdentifierExpression &&
                ((IdentifierExpression)targetMemberReference.Target).Identifier == context.Scope.Method.GetSimpleMemoryParameterName())
            {
                // This is a SimpleMemory access.

                var memberName = targetMemberReference.MemberName;

                var isWrite = memberName.StartsWith("Write");
                var invokationParameters = transformedParameters;
                invokationParameters.AddRange(new[]
                {
                    new DataObjectReference { DataObjectKind = DataObjectKind.Signal, Name = (isWrite ? SimpleMemoryPortNames.DataOut : SimpleMemoryPortNames.DataIn).ToExtendedVhdlId() },
                    new DataObjectReference { DataObjectKind = DataObjectKind.Signal, Name = (isWrite ? SimpleMemoryPortNames.WriteAddress : SimpleMemoryPortNames.ReadAddress).ToExtendedVhdlId() }
                });

                var target = "SimpleMemory" + targetMemberReference.MemberName;
                var memoryOperationInvokation = new Invokation
                {
                    Target = new Value { Content = target },
                    Parameters = invokationParameters
                };

                if (isWrite) return memoryOperationInvokation;

                // Looking up the type information that will tell us what the return type of the memory read is. This might
                // be some nodes up if e.g. there is an immediate cast expression.
                AstNode currentNode = expression;
                while (currentNode.Annotation<TypeInformation>() == null)
                {
                    currentNode = currentNode.Parent;
                }

                // If this is a memory read then comes the juggling with funneling the out parameter of the memory write
                // procedure to the original location.
                return BuildReturnReference(target, _typeConverter.ConvertTypeReference(currentNode.GetActualType()), memoryOperationInvokation, context, block);
            }


            var targetName = expression.GetFullName().ToExtendedVhdlId();

            context.TransformationContext.MethodCallChainTable.AddTarget(context.Scope.SubProgram.Name, targetName);

            var invokation = new Invokation
            {
                Target = targetName.ToVhdlIdValue(),
                Parameters = transformedParameters
            };


            // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to a non-void method).
            if (!(expression.Parent is ExpressionStatement))
            {
                AstType returnType;
                if (targetMemberReference != null)
                {
                    var targetFullName = expression.GetFullName();

                    returnType = targetMemberReference
                        .GetTargetType(context.TransformationContext.TypeDeclarationLookupTable)
                        .Members
                        .Single(member => member.GetFullName() == targetFullName)
                        .ReturnType;
                }
                else
                {
                    throw new NotSupportedException("Expressions having other than a MemberReferenceExpression as a target are not supported.");
                }

                return BuildReturnReference(targetName, _typeConverter.Convert(returnType), invokation, context, block);
            }
            else
            {
                // Simply return the procedure invokation if there is no return value.
                return invokation;
            }
        }

        private IVhdlElement TransformCastExpression(CastExpression expression, ISubTransformerContext context, IBlockElement block)
        {
            // This is a temporal workaround to get around cases where operations (e.g. multiplication) with 32b numbers resulting in a 64b number
            // would cause a cast to a 64b number type (what we don't support yet). See: https://lombiq.atlassian.net/browse/HAST-20
            var toTypeKeyword = ((PrimitiveType)expression.Type).Keyword;
            var fromTypeKeyword = expression.GetActualType().FullName;
            if (toTypeKeyword == "long" || toTypeKeyword == "ulong" || fromTypeKeyword == "System.Int64" || fromTypeKeyword == "System.UInt64")
            {
                return Transform(expression.Expression, context, block);
            }

            var fromType = _typeConverter.ConvertTypeReference(expression.GetActualType());
            var toType = _typeConverter.Convert(expression.Type);

            return ImplementTypeConversion(fromType, toType, Transform(expression.Expression, context, block));
        }

        private IVhdlElement ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement expression)
        {
            var castInvokation = new Invokation();

            // Trying supported cast scenarios:
            if ((fromType == KnownDataTypes.UnrangedInt || fromType == KnownDataTypes.Natural) && toType == KnownDataTypes.Real)
            {
                castInvokation.Target = new Raw("real");
            }
            else if ((fromType == KnownDataTypes.Real || fromType == KnownDataTypes.Natural) && (toType == KnownDataTypes.UnrangedInt || toType == KnownDataTypes.Natural))
            {
                castInvokation.Target = new Raw("integer");
            }
            else
            {
                throw new NotSupportedException("Casting from " + fromType.Name + " to " + toType.Name + " is not supported.");
            }

            castInvokation.Parameters.Add(expression);

            return castInvokation;
        }


        /// <summary>
        /// Making sure that the e.g. return variable names are unique per method call (to transfer procedure outputs).
        /// </summary>
        private static string GetNextUnusedTemporalVariableName(string targetName, string suffix, ISubTransformerContext context)
        {
            targetName = targetName.TrimExtendedVhdlIdDelimiters();

            var procedure = context.Scope.SubProgram;

            var variableName = (targetName + "." + suffix + "0").ToExtendedVhdlId();
            var returnVariableNameIndex = 0;
            while (procedure.Declarations.Any(declaration => declaration is Variable && ((Variable)declaration).Name == variableName))
            {
                variableName = (targetName + "." + suffix + ++returnVariableNameIndex).ToExtendedVhdlId();
            }

            return variableName;
        }

        private static DataObjectReference BuildReturnReference(string targetName, DataType returnType, Invokation invokation, ISubTransformerContext context, IBlockElement block)
        {
            var procedure = context.Scope.SubProgram;

            // Procedures can't just be assigned to variables like methods as they don't have a return value, just out parameters.
            // Thus here we create a variable for the out parameter (the return variable), run the procedure with it and then use it later too.
            var returnVariable = new Variable
            {
                Name = GetNextUnusedTemporalVariableName(targetName, "return", context),
                DataType = returnType
            };

            procedure.Declarations.Add(returnVariable);
            invokation.Parameters.Add(returnVariable.ToReference());

            // Adding the procedure invokation directly to the body so it's before the current expression...
            block.Body.Add(new Terminated(invokation));

            // ...and using the return variable in place of the original call.
            return returnVariable.ToReference();
        }
    }
}
