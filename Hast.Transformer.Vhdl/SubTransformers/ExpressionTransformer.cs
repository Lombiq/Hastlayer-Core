using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ExpressionTransformer
    {
        private readonly TypeConverter _typeConverter;

        public ExpressionTransformer()
            : this(new TypeConverter())
        {
        }

        public ExpressionTransformer(TypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        //  Would need to decide between + and & or sll/srl and sra/sla
        // See: http://www.csee.umbc.edu/portal/help/VHDL/operator.html
        public IVhdlElement Transform(Expression expression, SubTransformerContext context, IBlockElement block)
        {
            return new Raw(TransformInner(expression, context, block));
        }


        private string TransformInner(Expression expression, SubTransformerContext context, IBlockElement block)
        {
            if (expression is AssignmentExpression)
            {
                var assignment = (AssignmentExpression)expression;
                return TransformInner(assignment.Left, context, block) + " := " + TransformInner(assignment.Right, context, block);
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = (IdentifierExpression)expression;
                return identifier.Identifier.ToVhdlId();
            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = (PrimitiveExpression)expression;
                return primitive.Value.ToString();
            }
            else if (expression is BinaryOperatorExpression) return TransformBinaryOperatorExpression((BinaryOperatorExpression)expression, context, block);
            else if (expression is InvocationExpression) return TransformInvocationExpression((InvocationExpression)expression, context, block);
            // These are not needed at the moment.
            //else if (expression is MemberReferenceExpression)
            //{
            //    var memberReference = (MemberReferenceExpression)expression;
            //    return TransformInner(memberReference.Target, context, block) + "." + memberReference.MemberName;
            //}
            //else if (expression is ThisReferenceExpression)
            //{
            //    var thisRef = expression as ThisReferenceExpression;
            //    return context.Scope.Method.Parent.GetFullName();
            //}
            else if (expression is UnaryOperatorExpression)
            {
                var unary = expression as UnaryOperatorExpression;
                return "not (" + TransformInner(unary.Expression, context, block) + ")";
            }
            else if (expression is TypeReferenceExpression)
            {
                var type = ((TypeReferenceExpression)expression).Type;
                var declaration = context.TransformationContext.LookupDeclaration(type);

                if (declaration == null)
                {
                    throw new InvalidOperationException("No matching type for \"" + ((SimpleType)type).Identifier + "\" found in the syntax tree. This can mean that the type's assembly was not added to the syntax tree.");
                }

                return declaration.GetFullName();
            }
            else throw new NotSupportedException("Expressions of type " + expression.GetType() + " are not supported.");
        }

        private string TransformBinaryOperatorExpression(BinaryOperatorExpression expression, SubTransformerContext context, IBlockElement block)
        {
            var source = TransformInner(expression.Left, context, block) + " ";

            switch (expression.Operator)
            {
                case BinaryOperatorType.Add:
                    source += "+";
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
                    source += "/";
                    break;
                case BinaryOperatorType.Equality:
                    source += "=";
                    break;
                case BinaryOperatorType.ExclusiveOr:
                    source += "XOR";
                    break;
                case BinaryOperatorType.GreaterThan:
                    source += ">";
                    break;
                case BinaryOperatorType.GreaterThanOrEqual:
                    source += ">=";
                    break;
                case BinaryOperatorType.InEquality:
                    source += "/=";
                    break;
                case BinaryOperatorType.LessThan:
                    source += "<";
                    break;
                case BinaryOperatorType.LessThanOrEqual:
                    source += "<=";
                    break;
                case BinaryOperatorType.Modulus:
                    source += "mod";
                    break;
                case BinaryOperatorType.Multiply:
                    source += "*";
                    break;
                case BinaryOperatorType.NullCoalescing:
                    break;
                case BinaryOperatorType.ShiftLeft:
                    source += "sll";
                    break;
                case BinaryOperatorType.ShiftRight:
                    source += "srl";
                    break;
                case BinaryOperatorType.Subtract:
                    source += "-";
                    break;
            }

            return source + " " + TransformInner(expression.Right, context, block);
        }

        private string TransformInvocationExpression(InvocationExpression expression, SubTransformerContext context, IBlockElement block)
        {
            var procedure = context.Scope.SubProgram;
            var targetName = expression.GetFullName();
            var hasArguments = expression.Arguments.Count > 0;
            var hasReturnValue = !(expression.Parent is ExpressionStatement); // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to a non-void method)
            var needsParenthesis = hasArguments || hasReturnValue;

            context.TransformationContext.MethodCallChainTable.AddTarget(context.Scope.SubProgram.Name, targetName);

            var source = targetName.ToVhdlId();

            if (needsParenthesis) source += "(";

            if (hasArguments)
            {
                source += string.Join(", ", expression.Arguments.Select(argument => TransformInner(argument, context, block)));
            }

            var returnVariableName = string.Empty;

            if (hasReturnValue)
            {
                // Making sure that the return variable names are unique per method call.
                returnVariableName = targetName + ".ret0";
                var returnVariableNameIndex = 0;
                while (procedure.Declarations.Any(declaration => declaration is Variable && ((Variable)declaration).Name == returnVariableName))
                {
                    returnVariableName = targetName + ".ret" + ++returnVariableNameIndex;
                }


                AstType returnType;
                if (expression.Target is MemberReferenceExpression)
                {
                    var target = (MemberReferenceExpression)expression.Target;
                    TypeDeclaration type;

                    if (target.Target is TypeReferenceExpression)
                    {
                        // The method is in a different class.
                        type = context.TransformationContext.LookupDeclaration((TypeReferenceExpression)target.Target);
                    }
                    else
                    {
                        // The method is within this class.
                        AstNode current = expression;
                        while (!(current is TypeDeclaration))
                        {
                            current = current.Parent;
                        }

                        type = expression.GetParentType();
                    }

                    var targetMemberName = target.MemberName;

                    // Using First() because there can be multiple methods with the same name (overload) but these should have the same
                    // return type.
                    returnType = type.Members.First(member => member is MethodDeclaration && member.Name == targetMemberName).ReturnType;
                }
                else
                {
                    throw new NotSupportedException("Expressions having other than a MemberReferenceExpression as a target are not supported.");
                }

                procedure.Declarations.Add(new Variable
                {
                    Name = returnVariableName,
                    DataType = _typeConverter.Convert(returnType)
                });

                if (hasArguments) source += ",";
                source += returnVariableName.ToVhdlId();
            }

            if (needsParenthesis) source += ")";

            if (hasReturnValue)
            {
                source += ";";
                block.Body.Add(new Raw(source));
                source = returnVariableName.ToVhdlId();
            }

            return source;
        }
    }
}
