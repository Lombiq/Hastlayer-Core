using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Services
{
    public class BinaryAndUnaryOperatorExpressionsCastAdjuster : IBinaryAndUnaryOperatorExpressionsCastAdjuster
    {
        public void AdjustBinaryAndUnaryOperatorExpressions(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new BinaryAndUnaryOperatorExpressionsCastAdjusterVisitor());
        }


        private class BinaryAndUnaryOperatorExpressionsCastAdjusterVisitor : DepthFirstAstVisitor
        {
            private static BinaryOperatorType[] _binaryOperatorsWithNumericPromotions = new[]
            {
                BinaryOperatorType.Add,
                BinaryOperatorType.Subtract,
                BinaryOperatorType.Multiply,
                BinaryOperatorType.Divide,
                BinaryOperatorType.Modulus,
                BinaryOperatorType.BitwiseAnd,
                BinaryOperatorType.BitwiseOr,
                BinaryOperatorType.ExclusiveOr,
                BinaryOperatorType.Equality,
                BinaryOperatorType.InEquality,
                BinaryOperatorType.GreaterThan,
                BinaryOperatorType.LessThan,
                BinaryOperatorType.GreaterThanOrEqual,
                BinaryOperatorType.LessThanOrEqual
            };

            private static BinaryOperatorType[] _binaryOperatorsProducingNumericResults = new[]
            {
                BinaryOperatorType.Add,
                BinaryOperatorType.Subtract,
                BinaryOperatorType.Multiply,
                BinaryOperatorType.Divide,
                BinaryOperatorType.Modulus,
                BinaryOperatorType.BitwiseAnd,
                BinaryOperatorType.BitwiseOr,
                BinaryOperatorType.ExclusiveOr
            };

            private static string[] _numericTypes = new[]
            {
                typeof(byte).FullName,
                typeof(sbyte).FullName,
                typeof(short).FullName,
                typeof(ushort).FullName,
                typeof(int).FullName,
                typeof(uint).FullName,
                typeof(long).FullName,
                typeof(ulong).FullName,
            };

            // Those types that have arithmetic, relational and bitwise operations defined for them, see: 
            // https://github.com/dotnet/csharplang/blob/master/spec/expressions.md#arithmetic-operators
            private static string[] _numericTypesSupportingNumericPromotionOperations = new[]
            {
                typeof(int).FullName,
                typeof(uint).FullName,
                typeof(long).FullName,
                typeof(ulong).FullName,
            };

            private static UnaryOperatorType[] _unaryOperatorsWithNumericPromotions = new[]
            {
                UnaryOperatorType.Plus,
                UnaryOperatorType.Minus,
                UnaryOperatorType.BitNot
            };

            private static string[] _typesConvertedToIntInUnaryOperations = new[]
            {
                typeof(byte).FullName,
                typeof(sbyte).FullName,
                typeof(short).FullName,
                typeof(ushort).FullName,
                typeof(char).FullName
            };


            // Adding implicit casts as explained here: https://github.com/dotnet/csharplang/blob/master/spec/expressions.md#numeric-promotions

            public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
            {
                base.VisitBinaryOperatorExpression(binaryOperatorExpression);

                if (!_binaryOperatorsWithNumericPromotions.Contains(binaryOperatorExpression.Operator)) return;

                // If either type reference is null then most possibly that operand is a primitive value.

                var leftTypeReference = binaryOperatorExpression.Left.GetActualTypeReference();
                if (binaryOperatorExpression.Left is CastExpression)
                {
                    leftTypeReference = binaryOperatorExpression.Left.GetActualTypeReference(true);
                }

                var rightTypeReference = binaryOperatorExpression.Right.GetActualTypeReference();
                if (binaryOperatorExpression.Right is CastExpression)
                {
                    rightTypeReference = binaryOperatorExpression.Right.GetActualTypeReference(true);
                }


                // If no type reference can be determined then nothing to do.
                if (leftTypeReference == null && rightTypeReference == null) return;

                if (leftTypeReference == null) leftTypeReference = rightTypeReference;
                if (rightTypeReference == null) rightTypeReference = leftTypeReference;

                var leftTypeFullName = leftTypeReference.FullName;
                var rightTypeFullName = rightTypeReference.FullName;


                if (!_numericTypes.Contains(leftTypeFullName) || !_numericTypes.Contains(rightTypeFullName)) return;

                void castLeftToRight() => replaceLeft(rightTypeReference);

                void castRightToLeft() => replaceRight(leftTypeReference);

                void replaceLeft(TypeReference typeReference)
                {
                    binaryOperatorExpression.Left.ReplaceWith(CreateCast(typeReference, binaryOperatorExpression.Left));
                    setResultTypeReference(typeReference);
                }

                void replaceRight(TypeReference typeReference)
                {
                    binaryOperatorExpression.Right.ReplaceWith(CreateCast(typeReference, binaryOperatorExpression.Right));
                    setResultTypeReference(typeReference);
                }

                var resultTypeReferenceIsSet = false;
                void setResultTypeReference(TypeReference typeReference)
                {
                    if (resultTypeReferenceIsSet) return;
                    resultTypeReferenceIsSet = true;

                    // Changing the result type to align it with the operands' type (it will be always the same, but
                    // only for operations with numeric results, like +, -, but not for e.g. <=).
                    if (!_binaryOperatorsProducingNumericResults.Contains(binaryOperatorExpression.Operator))
                    {
                        return;
                    }

                    // We should also put a cast around it if necessary so it produces the same type as before.
                    if (!(binaryOperatorExpression.Parent is CastExpression))
                    {
                        var castExpression = CreateCast(binaryOperatorExpression.GetResultTypeReference(), binaryOperatorExpression);
                        binaryOperatorExpression.ReplaceWith(castExpression);
                        binaryOperatorExpression = (BinaryOperatorExpression)castExpression.Expression;
                    }

                    binaryOperatorExpression.ReplaceAnnotations(typeReference.ToTypeInformation());
                }


                var longFullName = typeof(long).FullName;
                var ulongFullName = typeof(ulong).FullName;
                var uintFullName = typeof(uint).FullName;
                var typesConvertedToLongForUint = new[] { typeof(sbyte).FullName, typeof(short).FullName, typeof(int).FullName };

                // Omitting decimal, double, float rules as those are not supported any way.
                if (leftTypeFullName == ulongFullName && rightTypeFullName != ulongFullName ||
                    leftTypeFullName != ulongFullName && rightTypeFullName == ulongFullName)
                {
                    if (leftTypeFullName == ulongFullName) castRightToLeft();
                    else castLeftToRight();
                }
                else if (leftTypeFullName == longFullName && rightTypeFullName != longFullName ||
                    leftTypeFullName != longFullName && rightTypeFullName == longFullName)
                {
                    if (leftTypeFullName == longFullName) castRightToLeft();
                    else castLeftToRight();
                }
                else if (leftTypeFullName == uintFullName && typesConvertedToLongForUint.Contains(rightTypeFullName) ||
                    rightTypeFullName == uintFullName && typesConvertedToLongForUint.Contains(leftTypeFullName))
                {
                    var longTypeReference = TypeHelper.CreateInt64TypeReference();
                    replaceLeft(longTypeReference);
                    replaceRight(longTypeReference);
                }
                else if (leftTypeFullName == uintFullName && rightTypeFullName != uintFullName ||
                    leftTypeFullName != uintFullName && rightTypeFullName == uintFullName)
                {
                    if (leftTypeFullName == uintFullName) castRightToLeft();
                    else castLeftToRight();
                }
                // While not specified under the numeric promotions language reference section, this condition cares 
                // about types that define all operators in questions. E.g. an equality check between two uints 
                // shouldn't force an int cast.
                else if (leftTypeFullName != rightTypeFullName ||
                    !_numericTypesSupportingNumericPromotionOperations.Contains(leftTypeFullName))
                {
                    var intTypeReference = TypeHelper.CreateInt32TypeReference();
                    replaceLeft(intTypeReference);
                    replaceRight(intTypeReference);
                }
            }

            public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
            {
                base.VisitUnaryOperatorExpression(unaryOperatorExpression);

                if (!_unaryOperatorsWithNumericPromotions.Contains(unaryOperatorExpression.Operator)) return;

                var typeReference = unaryOperatorExpression.Expression.GetActualTypeReference();
                var isCast = unaryOperatorExpression.Expression is CastExpression;
                var expectedTypeReference = unaryOperatorExpression.Expression.GetActualTypeReference(true);

                void replace(TypeReference newTypeReference)
                {
                    unaryOperatorExpression.Expression.ReplaceWith(CreateCast(newTypeReference, unaryOperatorExpression.Expression));

                    // We should also put a cast around it if necessary so it produces the same type as before.
                    if (!(unaryOperatorExpression.Parent is CastExpression))
                    {
                        var castExpression = CreateCast(typeReference, unaryOperatorExpression);
                        unaryOperatorExpression.ReplaceWith(castExpression);
                        unaryOperatorExpression = (UnaryOperatorExpression)castExpression.Expression;
                    }
                }

                if (_typesConvertedToIntInUnaryOperations.Contains(typeReference.FullName) &&
                    (!isCast || expectedTypeReference.FullName != typeof(int).FullName))
                {
                    replace(TypeHelper.CreateInt32TypeReference());
                }
                else if (unaryOperatorExpression.Operator == UnaryOperatorType.Minus &&
                    typeReference.FullName == typeof(uint).FullName &&
                    (!isCast || expectedTypeReference.FullName != typeof(long).FullName))
                {
                    replace(TypeHelper.CreateInt64TypeReference());
                }
                else if (unaryOperatorExpression.Operator == UnaryOperatorType.Minus &&
                    ((unaryOperatorExpression.Expression as CastExpression)?.Type as PrimitiveType)?.KnownTypeCode == KnownTypeCode.UInt32)
                {
                    // For an int value the AST can contain -(uint)value if the original code was (uint)-value.
                    // Fixing that here.
                    unaryOperatorExpression.Expression.ReplaceWith(((CastExpression)unaryOperatorExpression.Expression).Expression);
                }
            }


            private static CastExpression CreateCast(TypeReference toTypeReference, Expression expression)
            {
                var castExpression = new CastExpression { Type = AstType.Create(toTypeReference.FullName) };

                castExpression.Expression = expression.Clone();
                castExpression.AddAnnotation(toTypeReference.ToTypeInformation());

                return castExpression;
            }
        }
    }
}