using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation.Declaration;
using VhdlBuilder;
using VhdlBuilder.Representation.Expression;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl.SubTranspilers
{
    public class ExpressionTranspiler
    {
        // Passing in local scope with types, etc? Would be needed to decide between + and & or sll/srl and sra/sla
        // See: http://www.csee.umbc.edu/portal/help/VHDL/operator.html
        public IVhdlElement Transpile(Expression expression)
        {
            var raw = new Raw();

            if (expression is AssignmentExpression)
            {
                var assignment = expression as AssignmentExpression;
                // How should we handle signal assignments?
                raw.Source = Transpile(assignment.Left).ToVhdl() + " := " + Transpile(assignment.Right).ToVhdl();
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = expression as IdentifierExpression;
                raw.Source = identifier.Identifier.ToVhdlId();
            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = expression as PrimitiveExpression;
                raw.Source = primitive.Value.ToString();
            }
            else if (expression is BinaryOperatorExpression)
            {
                var binary = expression as BinaryOperatorExpression;

                raw.Source = Transpile(binary.Left).ToVhdl() + " ";

                switch (binary.Operator)
                {
                    case BinaryOperatorType.Add:
                        raw.Source += "+";
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
                        raw.Source += "/";
                        break;
                    case BinaryOperatorType.Equality:
                        raw.Source += "=";
                        break;
                    case BinaryOperatorType.ExclusiveOr:
                        raw.Source += "XOR";
                        break;
                    case BinaryOperatorType.GreaterThan:
                        raw.Source += ">";
                        break;
                    case BinaryOperatorType.GreaterThanOrEqual:
                        raw.Source += ">=";
                        break;
                    case BinaryOperatorType.InEquality:
                        raw.Source += "/=";
                        break;
                    case BinaryOperatorType.LessThan:
                        raw.Source += "<";
                        break;
                    case BinaryOperatorType.LessThanOrEqual:
                        raw.Source += "<=";
                        break;
                    case BinaryOperatorType.Modulus:
                        raw.Source += "mod";
                        break;
                    case BinaryOperatorType.Multiply:
                        raw.Source += "*";
                        break;
                    case BinaryOperatorType.NullCoalescing:
                        break;
                    case BinaryOperatorType.ShiftLeft:
                        raw.Source += "sll";
                        break;
                    case BinaryOperatorType.ShiftRight:
                        raw.Source += "srl";
                        break;
                    case BinaryOperatorType.Subtract:
                        raw.Source += "-";
                        break;
                }

                raw.Source += " " + Transpile(binary.Right).ToVhdl();
            }

            return raw;
        }
    }
}
