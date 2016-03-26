using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class CompilerGeneratedClassesConverter : ICompilerGeneratedClassesConverter
    {
        public void InlineCompilerGeneratedClasses(SyntaxTree syntaxTree)
        {
            /*  Inlining DisplayClasses. E.g. this:
            
                private sealed class <>c__DisplayClass2
                {
                    public uint[] numbers;

                    public PrimeCalculator <>4__this;

                    public bool <ParallelizedArePrimeNumbersAsync>b__0 (object indexObject)
                    {
                        return this.<>4__this.IsPrimeNumberInternal (this.numbers [(int)indexObject]);
                    }
                }
                
                Will become this (in the parent class):
              
                public bool <ParallelizedArePrimeNumbersAsync>b__0 (uint[] numbers, object indexObject)
                {
                    return this.IsPrimeNumberInternal (numbers [(int)indexObject]);
                }
             */

            // Processing the DisplayClasses themselves.
            var compilerGeneratedClasses = syntaxTree
                .GetAllTypeDeclarations()
                .Where(type => type.ClassType == ClassType.Class && type.Name.Contains("__DisplayClass"))
                .Where(type => type
                    .Attributes
                    .Any(attributeSection => attributeSection
                        .Attributes.Any(attribute => attribute.Type.GetSimpleName() == "CompilerGeneratedAttribute")));
            var thisFieldNames = new HashSet<string>();
            foreach (var compilerGeneratedClass in compilerGeneratedClasses)
            {
                // Removing __this references.
                var thisField = compilerGeneratedClass.Members
                    .SingleOrDefault(member =>
                        member is FieldDeclaration &&
                        ((FieldDeclaration)member).Variables.Any(variable => variable.Name.EndsWith("__this")));
                if (thisField != null)
                {
                    var fieldName = ((FieldDeclaration)thisField).Variables.First().Name;
                    thisFieldNames.Add(fieldName);
                    thisField.Remove();
                    compilerGeneratedClass.AcceptVisitor(new DisplayClassThisFieldReferenceRemovingVisitor(fieldName));
                }
            }


            // Processing consumer code of DisplayClasses.
            syntaxTree.AcceptVisitor(new ThisFieldReferenceRemovingVisitor(thisFieldNames));
        }


        private class DisplayClassThisFieldReferenceRemovingVisitor : DepthFirstAstVisitor
        {
            private readonly string _thisFieldName;


            public DisplayClassThisFieldReferenceRemovingVisitor(string thisFieldName)
            {
                _thisFieldName = thisFieldName;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                if (memberReferenceExpression.MemberName == _thisFieldName)
                {
                    // The parent should also be a MemberReferenceExpression since there is no other type of access
                    // to __this fields.
                    var parentMemberReferenceExpression = (MemberReferenceExpression)memberReferenceExpression.Parent;
                    parentMemberReferenceExpression.Target = memberReferenceExpression.Target;
                    memberReferenceExpression.Remove();
                }
            }
        }

        private class ThisFieldReferenceRemovingVisitor : DepthFirstAstVisitor
        {
            private readonly HashSet<string> _thisFieldNames;


            public ThisFieldReferenceRemovingVisitor(HashSet<string> thisFieldNames)
            {
                _thisFieldNames = thisFieldNames;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                if (_thisFieldNames.Contains(memberReferenceExpression.MemberName))
                {
                    // Removing the whole statement which should be something like:
                    // <>c__DisplayClass.<>4__this = this;
                    memberReferenceExpression.FindFirstParentOfType<ExpressionStatement>().Remove();
                }
            }
        }
    }
}
