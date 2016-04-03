﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

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

            var compilerGeneratedClasses = syntaxTree
                .GetAllTypeDeclarations()
                .Where(type => type.ClassType == ClassType.Class && type.Name.Contains("__DisplayClass"))
                .Where(type => type
                    .Attributes
                    .Any(attributeSection => attributeSection
                        .Attributes.Any(attribute => attribute.Type.GetSimpleName() == "CompilerGeneratedAttribute")));
            foreach (var compilerGeneratedClass in compilerGeneratedClasses)
            {
                var parentClass = compilerGeneratedClass.FindFirstParentTypeDeclaration();

                var fields = compilerGeneratedClass.Members.OfType<FieldDeclaration>()
                    .OrderBy(field => field.Variables.Single().Name)
                    .ToDictionary(field => field.Variables.Single().Name);

                // Processing the DisplayClasses themselves.
                {
                    // Removing __this references.
                    var thisField = compilerGeneratedClass.Members
                        .SingleOrDefault(member =>
                            member is FieldDeclaration &&
                            ((FieldDeclaration)member).Variables.Any(variable => variable.Name.EndsWith("__this")));
                    if (thisField != null)
                    {
                        var fieldName = ((FieldDeclaration)thisField).Variables.First().Name;

                        thisField.Remove();

                        Action<MemberReferenceExpression> memberReferenceExpressionProcessor = memberReferenceExpression =>
                            {
                                if (memberReferenceExpression.MemberName == fieldName)
                                {
                                    // The parent should also be a MemberReferenceExpression since there is no other type of access
                                    // to __this fields.
                                    var parentMemberReferenceExpression = (MemberReferenceExpression)memberReferenceExpression.Parent;
                                    parentMemberReferenceExpression.Target = memberReferenceExpression.Target;
                                    memberReferenceExpression.Remove();
                                }
                            };
                        compilerGeneratedClass
                            .AcceptVisitor(new MemberReferenceExpressionVisitingVisitor(memberReferenceExpressionProcessor));
                    }


                    // Converting fields into method arguments.

                    foreach (var field in fields.Values)
                    {
                        field.Remove();
                    }

                    foreach (var method in compilerGeneratedClass.Members.OfType<MethodDeclaration>())
                    {
                        // Adding parameters for every field that the method used, in alphabetical order, and changing field
                        // references to parameter references.

                        var parametersForFormerFields = new SortedList<string, ParameterDeclaration>();

                        Action<MemberReferenceExpression> memberReferenceExpressionProcessor = memberReferenceExpression =>
                            {
                                var fieldDefinition = memberReferenceExpression.Annotation<FieldDefinition>();

                                if (fieldDefinition == null) return;

                                var field = fields.Values
                                    .Single(f => f.Annotation<FieldDefinition>().FullName == fieldDefinition.FullName);

                                // Is the field assigned to? Because we don't support that currently, since with it being
                                // converted to a parameter we'd need to return its value and assign it to the caller's
                                // variable. Maybe we'll allow this with static field support, but not for lambdas used
                                // in parallelized expressions (since that would require concurrent access too).
                                var isAssignedTo =
                                    // The field is directly assigned to.
                                    (memberReferenceExpression.Parent is AssignmentExpression &&
                                    ((AssignmentExpression)memberReferenceExpression.Parent).Left == memberReferenceExpression)
                                    ||
                                    // The field's indexed element is assigned to.
                                    (memberReferenceExpression.Parent is IndexerExpression &&
                                    memberReferenceExpression.Parent.Parent is AssignmentExpression &&
                                    ((AssignmentExpression)memberReferenceExpression.Parent.Parent).Left == memberReferenceExpression.Parent);
                                if (isAssignedTo)
                                {
                                    throw new NotSupportedException("It's not supported to modify the content of a variable coming from the parent scope in a lambda expression. Pass arguments instead.");
                                }

                                // This generated name should be sufficiently unique so it doesn't clash with existing
                                // parameters.
                                var parameterName = CreateGeneratedClassVariableName(
                                    compilerGeneratedClass, 
                                    field.Variables.Single().Name);

                                // Adding a method parameter for the field.
                                parametersForFormerFields.Add(
                                    parameterName,
                                    new ParameterDeclaration(field.ReturnType.Clone(), parameterName));

                                // Changing the field reference to a reference to the parameter.
                                memberReferenceExpression.ReplaceWith(new IdentifierExpression(parameterName));
                            };
                        method.AcceptVisitor(new MemberReferenceExpressionVisitingVisitor(memberReferenceExpressionProcessor));

                        method.Parameters.AddRange(parametersForFormerFields.Values);

                        // Moving the method to the parent class.
                        method.Name = compilerGeneratedClass.Name + "_" + method.Name;
                        parentClass.Members.Add((MethodDeclaration)method.Clone());
                        compilerGeneratedClass.Remove();
                    }
                }

                // Processing consumer code of DisplayClasses.
                {
                    var compilerGeneratedClassTypeInfoName = compilerGeneratedClass.Annotation<TypeDefinition>().FullName;
                    var compilerGeneratClassFullName = compilerGeneratedClass.GetFullName();
                    var variablesAlreadyDeclared = new HashSet<string>();

                    Action<MemberReferenceExpression> memberReferenceExpressionProcessor = memberReferenceExpression =>
                    {
                        if (memberReferenceExpression.Target is IdentifierExpression &&
                            memberReferenceExpression.Target.Annotation<TypeInformation>().ExpectedType.FullName == compilerGeneratedClassTypeInfoName)
                        {
                            var memberName = memberReferenceExpression.MemberName;

                            // Removing __this field references.
                            if (memberName.EndsWith("__this"))
                            {
                                // Removing the whole statement which should be something like:
                                // <>c__DisplayClass.<>4__this = this;
                                memberReferenceExpression.FindFirstParentOfType<ExpressionStatement>().Remove();
                            }
                            // Converting DisplayClass field references to local variable references.
                            else
                            {
                                var variableName = CreateGeneratedClassVariableName(compilerGeneratedClass, memberName);

                                // Adding a variable declaration for the variable.
                                var variableIdentifier = memberReferenceExpression
                                    .FindFirstParentOfType<MethodDeclaration>()
                                    .GetFullName() + variableName;
                                if (!variablesAlreadyDeclared.Contains(variableIdentifier))
                                {
                                    variablesAlreadyDeclared.Add(variableIdentifier);

                                    var variableDeclaration = new VariableDeclarationStatement(
                                        fields[memberName].ReturnType.Clone(), variableName);

                                    // We need to find the variable declaration of the DisplayClass object and insert
                                    // such declarations adjacent to it. This will ensure that the new variable
                                    // declarations have the same scope.
                                    var parentMethod = memberReferenceExpression.FindFirstParentOfType<MethodDeclaration>();
                                    var displayClassVariableDeclaration = parentMethod.Descendants
                                        .Single(node => 
                                            {
                                                if (!(node is VariableDeclarationStatement)) return false;

                                                var variableType = ((VariableDeclarationStatement)node).Type;

                                                if (!(variableType is MemberType)) return false;

                                                return variableType.GetFullName() == compilerGeneratClassFullName;
                                            });

                                    displayClassVariableDeclaration.Parent.InsertChildAfter(
                                        displayClassVariableDeclaration, variableDeclaration, BlockStatement.StatementRole);
                                }

                                var variableIdentifierExpression = new IdentifierExpression(variableName);
                                memberReferenceExpression.ReplaceWith(variableIdentifierExpression);
                            }
                        }
                    };

                    parentClass.AcceptVisitor(new MemberReferenceExpressionVisitingVisitor(memberReferenceExpressionProcessor));
                }
            }
        }


        private static string CreateGeneratedClassVariableName(TypeDeclaration compilerGeneratedClass, string variableName)
        {
            return compilerGeneratedClass.Name + "_" + variableName;
        }


        private class MemberReferenceExpressionVisitingVisitor : DepthFirstAstVisitor
        {
            private readonly Action<MemberReferenceExpression> _expressionProcessor;


            public MemberReferenceExpressionVisitingVisitor(Action<MemberReferenceExpression> expressionProcessor)
            {
                _expressionProcessor = expressionProcessor;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);
                _expressionProcessor(memberReferenceExpression);
            }
        }
    }
}
