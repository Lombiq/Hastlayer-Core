//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ICSharpCode.NRefactory.CSharp
//{
//    public static class IdentifierExpressionExtensions
//    {
//        public static VariableInitializer FindInitializer(this IdentifierExpression expression)
//        {
//            var expressionFullName = expression.GetFullName();
//            var parentBlock = expression.FindFirstParentOfType<BlockStatement>();
//            var variableInitializer = parentBlock?
//                .Statements
//                .SingleOrDefault(statement => 
//                    statement.Is<VariableDeclarationStatement>(declaration => declaration.Variables.SingleOrDefault(initializer => initializer.GetFullName() == expressionFullName)));
//            return null;
//        }


//        //private static VariableDeclarationStatement FindMatchinStatement()
//        //{

//        //}
//    }
//}
