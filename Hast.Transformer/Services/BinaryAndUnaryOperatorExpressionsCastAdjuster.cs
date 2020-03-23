using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class BinaryAndUnaryOperatorExpressionsCastAdjuster : IBinaryAndUnaryOperatorExpressionsCastAdjuster
    {
        public void AdjustBinaryAndUnaryOperatorExpressions(SyntaxTree syntaxTree, IKnownTypeLookupTable knownTypeLookupTable)
        {
            syntaxTree.AcceptVisitor(new BinaryAndUnaryOperatorExpressionsCastAdjusterVisitor(knownTypeLookupTable));
        }


        private class BinaryAndUnaryOperatorExpressionsCastAdjusterVisitor : DepthFirstAstVisitor
        {
            // Note that while shifts are missing from the list under 
            // https://github.com/dotnet/csharplang/blob/master/spec/expressions.md#numeric-promotions they still get
            // kind of a numeric promotion. See the page about bitwise and shift operators
            // (https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
            // mentions that "When operands are of different integral types, their values are converted to the closest
            // containing integral type."
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
                BinaryOperatorType.LessThanOrEqual,
                BinaryOperatorType.ShiftLeft,
                BinaryOperatorType.ShiftRight
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
                BinaryOperatorType.ExclusiveOr,
                BinaryOperatorType.ShiftLeft,
                BinaryOperatorType.ShiftRight
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
                typeof(ulong).FullName
            };

            // Those types that have arithmetic, relational and bitwise operations defined for them, see: 
            // https://github.com/dotnet/csharplang/blob/master/spec/expressions.md#arithmetic-operators
            private static string[] _numericTypesSupportingNumericPromotionOperations = new[]
            {
                typeof(int).FullName,
                typeof(uint).FullName,
                typeof(long).FullName,
                typeof(ulong).FullName
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

            private readonly IKnownTypeLookupTable _knownTypeLookupTable;


            public BinaryAndUnaryOperatorExpressionsCastAdjusterVisitor(IKnownTypeLookupTable knownTypeLookupTable)
            {
                _knownTypeLookupTable = knownTypeLookupTable;
            }


            // Adding implicit casts as explained here: 
            // https://github.com/dotnet/csharplang/blob/master/spec/expressions.md#numeric-promotions
            // Also handling shifts where the left operand needs to be u/int or u/long, see: 
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators

            public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
            {
                base.VisitBinaryOperatorExpression(binaryOperatorExpression);

                if (!_binaryOperatorsWithNumericPromotions.Contains(binaryOperatorExpression.Operator)) return;

                var leftType = binaryOperatorExpression.Left.GetActualType();
                if (binaryOperatorExpression.Left is CastExpression)
                {
                    leftType = binaryOperatorExpression.Left.GetActualType();
                }

                var rightType = binaryOperatorExpression.Right.GetActualType();
                if (binaryOperatorExpression.Right is CastExpression)
                {
                    rightType = binaryOperatorExpression.Right.GetActualType();
                }


                // If no type can be determined then nothing to do.
                if (leftType == null && rightType == null) return;

                if (leftType == null) leftType = rightType;
                if (rightType == null) rightType = leftType;

                var leftTypeFullName = leftType.GetFullName();
                var rightTypeFullName = rightType.GetFullName();


                if (!_numericTypes.Contains(leftTypeFullName) || !_numericTypes.Contains(rightTypeFullName)) return;

                void castLeftToRight() => replaceLeft(rightType);

                void castRightToLeft() => replaceRight(leftType);

                void replaceLeft(IType type)
                {
                    binaryOperatorExpression.Left.ReplaceWith(CreateCast(type, binaryOperatorExpression.Left, out var _));
                    setResultTypeReference(type);
                }

                void replaceRight(IType type)
                {
                    binaryOperatorExpression.Right.ReplaceWith(CreateCast(type, binaryOperatorExpression.Right, out var _));
                    setResultTypeReference(type);
                }

                var resultTypeReferenceIsSet = false;
                void setResultTypeReference(IType type)
                {
                    if (resultTypeReferenceIsSet) return;
                    resultTypeReferenceIsSet = true;

                    // Changing the result type to align it with the operands' type (it will be always the same, but
                    // only for operations with numeric results, like +, -, but not for e.g. <=).
                    if (!_binaryOperatorsProducingNumericResults.Contains(binaryOperatorExpression.Operator))
                    {
                        return;
                    }

                    // We should also put a cast around it if necessary so it produces the same type as before. But only
                    // if this binary operator expression is not also in another binary operator expression, when it 
                    // will be cast again.
                    var firstNonParenthesizedExpressionParent = binaryOperatorExpression.FindFirstNonParenthesizedExpressionParent();
                    if (!(firstNonParenthesizedExpressionParent is CastExpression) &&
                        !(firstNonParenthesizedExpressionParent is BinaryOperatorExpression))
                    {
                        var castExpression = CreateCast(
                            binaryOperatorExpression.GetResultType(),
                            binaryOperatorExpression,
                            out var clonedBinaryOperatorExpression);
                        binaryOperatorExpression.ReplaceWith(castExpression);
                        binaryOperatorExpression = clonedBinaryOperatorExpression;
                    }

                    binaryOperatorExpression.ReplaceAnnotations(type.ToResolveResult());
                }


                var longFullName = typeof(long).FullName;
                var ulongFullName = typeof(ulong).FullName;
                var intFullName = typeof(int).FullName;
                var uintFullName = typeof(uint).FullName;
                var typesConvertedToLongForUint = new[] { typeof(sbyte).FullName, typeof(short).FullName, intFullName };

                // First handling shifts which are different from other affected binary operators because only the 
                // left operand is promoted, and only everything below int to int.
                if (binaryOperatorExpression.Operator == BinaryOperatorType.ShiftLeft ||
                    binaryOperatorExpression.Operator == BinaryOperatorType.ShiftRight)
                {

                    if (!new[] { longFullName, ulongFullName, intFullName, uintFullName }.Contains(leftTypeFullName))
                    {
                        replaceLeft(_knownTypeLookupTable.Lookup(KnownTypeCode.Int32));
                    }
                }
                else
                {
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
                        var longType = _knownTypeLookupTable.Lookup(KnownTypeCode.Int64);
                        replaceLeft(longType);
                        replaceRight(longType);
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
                        var intType = _knownTypeLookupTable.Lookup(KnownTypeCode.Int32);
                        replaceLeft(intType);
                        replaceRight(intType);
                    }
                }
            }

            public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
            {
                base.VisitUnaryOperatorExpression(unaryOperatorExpression);

                if (!_unaryOperatorsWithNumericPromotions.Contains(unaryOperatorExpression.Operator)) return;

                var type = unaryOperatorExpression.Expression.GetActualType();
                var isCast = unaryOperatorExpression.Expression is CastExpression;
                var expectedType = unaryOperatorExpression.Expression.GetActualType();

                void replace(IType newType)
                {
                    unaryOperatorExpression.Expression.ReplaceWith(CreateCast(newType, unaryOperatorExpression.Expression, out var _));
                }

                if (_typesConvertedToIntInUnaryOperations.Contains(type.GetFullName()) &&
                    (!isCast || expectedType.GetFullName() != typeof(int).FullName))
                {
                    replace(_knownTypeLookupTable.Lookup(KnownTypeCode.Int32));
                }
                else if (unaryOperatorExpression.Operator == UnaryOperatorType.Minus &&
                    type.GetFullName() == typeof(uint).FullName &&
                    (!isCast || expectedType.GetFullName() != typeof(long).FullName))
                {
                    replace(_knownTypeLookupTable.Lookup(KnownTypeCode.Int64));
                }
                else if (unaryOperatorExpression.Operator == UnaryOperatorType.Minus &&
                    ((unaryOperatorExpression.Expression as CastExpression)?.Type as PrimitiveType)?.KnownTypeCode == KnownTypeCode.UInt32)
                {
                    // For an int value the AST can contain -(uint)value if the original code was (uint)-value.
                    // Fixing that here.
                    unaryOperatorExpression.Expression.ReplaceWith(((CastExpression)unaryOperatorExpression.Expression).Expression);
                }
            }


            private static CastExpression CreateCast<T>(IType toType, T expression, out T clonedExpression)
                where T : Expression
            {
                var castExpression = new CastExpression { Type = TypeHelper.CreateAstType(toType) };

                clonedExpression = expression.Clone<T>();
                castExpression.Expression = new ParenthesizedExpression(clonedExpression);
                castExpression.Expression.AddAnnotation(expression.CreateResolveResultFromActualType());
                castExpression.AddAnnotation(toType.ToResolveResult());

                return castExpression;
            }
        }
    }
}