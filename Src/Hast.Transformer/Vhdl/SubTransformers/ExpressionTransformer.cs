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
                var assignment = expression as AssignmentExpression;
                return TransformInner(assignment.Left, context, block) + " := " + TransformInner(assignment.Right, context, block);
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = expression as IdentifierExpression;
                return identifier.Identifier.ToVhdlId();
            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = expression as PrimitiveExpression;
                return primitive.Value.ToString();
            }
            else if (expression is BinaryOperatorExpression) return TransformBinaryOperatorExpression((BinaryOperatorExpression)expression, context, block);
            else if (expression is InvocationExpression) return TransformInvocationExpression((InvocationExpression)expression, context, block);
            else if (expression is MemberReferenceExpression)
            {
                var member = expression as MemberReferenceExpression;
                return TransformInner(member.Target, context, block) + "." + member.MemberName;
            }
            else if (expression is ThisReferenceExpression)
            {
                var thisRef = expression as ThisReferenceExpression;
                return NameUtility.GetFullName(context.Scope.Node.Parent);
            }
            else if (expression is UnaryOperatorExpression)
            {
                var unary = expression as UnaryOperatorExpression;
                return "not (" + TransformInner(unary.Expression, context, block) + ")";
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
            var targetName = TransformInner(expression.Target, context, block);
            var hasArguments = expression.Arguments.Count > 0;
            var hasReturnValue = !(expression.Parent is ExpressionStatement); // If the parent is not an ExpressionStatement then the invocation's result is needed (i.e. the call is to a non-void method)
            var needsParenthesis = hasArguments || hasReturnValue;

            context.TransformingContext.CallChainTable.AddTarget(context.Scope.SubProgram.Name, targetName);

            var source = targetName.ToVhdlId();

            if (needsParenthesis) source += "(";

            if (hasArguments)
            {
                source += string.Join(", ", expression.Arguments.Select(argument => TransformInner(argument, context, block)));
            }

            if (hasReturnValue)
            {
                var returnVarName = targetName + ".ret";

                // Checking whether a variable for this return value exists
                if (!procedure.Declarations
                    .Any(element =>
                    {
                        if (!(element is Hast.VhdlBuilder.Representation.Declaration.DataObject)) return false;

                        return ((Hast.VhdlBuilder.Representation.Declaration.DataObject)element).Name == returnVarName;
                    }))
                {
                    // This is expensive, any better way?
                    var targetNode = context.TransformingContext.SyntaxTree.Descendants
                                        .Where(node => node is MethodDeclaration)
                                        .Where(node => NameUtility.GetFullName(node) == targetName)
                                        .Single();

                    procedure.Declarations.Add(new Variable
                    {
                        Name = returnVarName,
                        DataType = _typeConverter.Convert(((MethodDeclaration)targetNode).ReturnType)
                    });
                }

                if (hasArguments) source += ",";
                source += returnVarName.ToVhdlId();
            }

            if (needsParenthesis) source += ")";

            if (hasReturnValue)
            {
                source += ";";
                block.Body.Add(new Raw(source));
                source = (targetName + ".ret").ToVhdlId();
            }

            return source;
        }
    }
}
