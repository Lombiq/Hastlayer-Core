﻿using System.Linq;
using System.Text;

namespace Hast.TestInputs.Dynamic
{
    public static class BinaryAndUnaryOperatorExpressionCasesGenerator
    {
        public static void Generate()
        {
            var needsShiftCastTypes = new[] { "uint", "long", "ulong" };
            var types = new[] { "byte", "sbyte", "short", "ushort", "int" }.Union(needsShiftCastTypes).ToArray();
            var codeBuilder = new StringBuilder();

            var memoryIndex = 0;
            string AddSaveResult(string code, bool addLongCast = true)
            {
                var originalMemoryIndex = memoryIndex;
                memoryIndex += 2;
                // The long cast will be unnecessary most of the time but determining when it's needed is complex so
                // good enough.
                return $@"SaveResult(memory, {originalMemoryIndex}, {(addLongCast ? "(long)(" : string.Empty)}{code}{(addLongCast ? ")" : string.Empty)});";
            }

            string AddiSaveResults(
                string code0,
                string code1,
                string code2,
                string code3,
                string code4,
                string code5,
                string code6,
                string code7)
            {
                var originalMemoryIndex = memoryIndex;
                memoryIndex += 16;
                // The long cast will be unnecessary most of the time but determining when it's needed is complex so
                // good enough.
                // Awkward indentation is so the generated code can be properly formatted.
                return $@"SaveResult(
    memory, 
    {originalMemoryIndex}, 
    (long)({code0}), // {originalMemoryIndex}
    (long)({code1}), // {originalMemoryIndex + 2}
    (long)({code2}), // {originalMemoryIndex + 4}
    (long)({code3}), // {originalMemoryIndex + 6}
    (long)({code4}), // {originalMemoryIndex + 8}
    (long)({code5}), // {originalMemoryIndex + 10}
    (long)({code6}), // {originalMemoryIndex + 12}
    (long)({code7})); // {originalMemoryIndex + 14}";
            }

            for (int i = 0; i < types.Length; i++)
            {
                memoryIndex = 0;
                var leftType = types[i];
                var left = leftType + "Left";

                void AddMethodStart(string nameSuffix)
                {
                    codeBuilder.AppendLine($@"
                        public virtual void {leftType.ToUpper()[0]}{leftType.Substring(1)}BinaryOperatorExpressionVariations{nameSuffix}(SimpleMemory memory)
                        {{");

                    if (leftType == "long" || leftType == "ulong")
                    {
                        codeBuilder.AppendLine($@"{leftType} input = (({leftType})memory.ReadInt32(0) << 32) | (uint)memory.ReadInt32(1);
                        var {left} = input;");
                    }
                    else if (leftType == "uint")
                    {
                        codeBuilder.AppendLine($@"var input = memory.ReadUInt32(0);
                        var {left} = input;");
                    }
                    else
                    {
                        codeBuilder.AppendLine($@"var input = memory.ReadInt32(0);
                        var {left} = {(leftType != "int" ? $"({leftType})" : string.Empty)}input;");
                    }
                }

                if (leftType == "long" || leftType == "ulong")
                {
                    AddMethodStart("Low");
                }
                else
                {
                    AddMethodStart(string.Empty);
                }

                for (int j = 0; j < types.Length; j++)
                {
                    if ((leftType == "long" || leftType == "ulong") && j == types.Length / 2)
                    {
                        codeBuilder.AppendLine("}");
                        AddMethodStart("High");
                    }

                    var rightType = types[j];
                    var right = rightType + "Right";

                    codeBuilder.AppendLine($@"
                        var {right} = {(rightType != "int" || leftType.Contains("long") || leftType == "uint" ? $"({rightType})" : string.Empty)}input;");

                    var shiftRightOperandCast = needsShiftCastTypes.Contains(rightType) ? "(int)" : string.Empty;

                    codeBuilder.AppendLine(AddSaveResult($"{left} << {shiftRightOperandCast}{right}"));
                    codeBuilder.AppendLine(AddSaveResult($"{left} >> {shiftRightOperandCast}{right}"));

                    // These variations can't be applied to the operands directly except for shift (which has a cast). 
                    if (!(
                        leftType == "sbyte" && rightType == "ulong" ||
                        leftType == "short" && rightType == "ulong" ||
                        leftType == "int" && rightType == "ulong" ||
                        leftType == "long" && rightType == "ulong" ||
                        leftType == "ulong" && rightType == "sbyte" ||
                        leftType == "ulong" && rightType == "short" ||
                        leftType == "ulong" && rightType == "int" ||
                        leftType == "ulong" && rightType == "long"))
                    {
                        codeBuilder.AppendLine(AddiSaveResults(
                            $"{left} + {right}",
                            $"{left} - {right}",
                            $"{left} * {right}",
                            $"{left} / {right}",
                            $"{left} % {right}",
                            $"{left} & {right}",
                            $"{left} | {right}",
                            $"{left} ^ {right}"));
                    }
                }

                codeBuilder.AppendLine("}");
            }

            System.IO.File.WriteAllText("AllBinaryOperatorExpressionVariations.cs", codeBuilder.ToString());

            memoryIndex = 0;
            codeBuilder.Clear();

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var isUlong = type == "ulong";
                var variableName = type + "Operand";

                codeBuilder.AppendLine($@"
                    var {variableName} = {(type != "long" ? $"({type})" : string.Empty)}input;");

                codeBuilder.AppendLine(AddSaveResult("~" + variableName, isUlong));
                codeBuilder.AppendLine(AddSaveResult("+" + variableName, isUlong));

                if (type != "ulong")
                {
                    codeBuilder.AppendLine(AddSaveResult("-" + variableName, isUlong));
                }
            }

            System.IO.File.WriteAllText("AllUnaryOperatorExpressionVariations.cs", codeBuilder.ToString());


        }
    }
}