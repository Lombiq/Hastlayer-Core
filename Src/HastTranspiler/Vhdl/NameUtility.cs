using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace HastTranspiler.Vhdl
{
    public static class NameUtility
    {
        public static string GetFullName(AstNode node)
        {
            var nameBuilder = new StringBuilder();
            
            var currentNode = node;
            var stop = false;
            while (!stop && !(currentNode is SyntaxTree))
            {
                var type = currentNode.GetType();

                var prop = type.GetProperty("Name");
                if (prop == null) prop = type.GetProperty("Identifier");

                if (prop != null) nameBuilder.Insert(0, prop.GetValue(currentNode)).Insert(0, ".");
                else stop = true;

                currentNode = currentNode.Parent;
            }

            var fullName = nameBuilder.ToString();
            return fullName.Length > 0 ? fullName.Substring(1) : fullName; // Cutting off leading dot if necessary
        }
    }
}
