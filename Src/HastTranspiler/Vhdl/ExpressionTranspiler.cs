using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl
{
    public static class ExpressionTranspiler
    {
        public static IVhdlElement Transpile(Expression expression)
        {
            var raw = new Raw();

            if (expression is AssignmentExpression)
            {
                var assignment = expression as AssignmentExpression;
                // How should we handle signal assignments?
                raw.Source = Transpile(assignment.Left).ToVhdl() + " := " + Transpile(assignment.Right).ToVhdl() + ";";
            }
            else if (expression is IdentifierExpression)
            {
                var identifier = expression as IdentifierExpression;
                raw.Source = identifier.Identifier;
            }
            else if (expression is PrimitiveExpression)
            {
                var primitive = expression as PrimitiveExpression;
                raw.Source = primitive.Value.ToString();
            }

            return raw;
        }
    }
}
