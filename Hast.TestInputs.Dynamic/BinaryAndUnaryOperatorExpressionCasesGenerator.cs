using System.Linq;
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
            string AddMemoryWrite(string variableName)
            {
                var originalMemoryIndex = memoryIndex;
                memoryIndex += 2;
                // The long cast will be unnecessary most of the time but determining when it's needed is complex so
                // good enough.
                return $@"SaveResult(memory, {originalMemoryIndex}, (long)({variableName}));";
            }

            string Add8MemoryWrites(
                string variableName0,
                string variableName1,
                string variableName2,
                string variableName3,
                string variableName4,
                string variableName5,
                string variableName6,
                string variableName7)
            {
                var originalMemoryIndex = memoryIndex;
                memoryIndex += 16;
                // The long cast will be unnecessary most of the time but determining when it's needed is complex so
                // good enough.
                // Awkward indentation is so the generated code can be properly formatted.
                return $@"SaveResult(
    memory, 
    {originalMemoryIndex}, 
    (long)({variableName0}), // {originalMemoryIndex}
    (long)({variableName1}), // {originalMemoryIndex + 2}
    (long)({variableName2}), // {originalMemoryIndex + 4}
    (long)({variableName3}), // {originalMemoryIndex + 6}
    (long)({variableName4}), // {originalMemoryIndex + 8}
    (long)({variableName5}), // {originalMemoryIndex + 10}
    (long)({variableName6}), // {originalMemoryIndex + 12}
    (long)({variableName7})); // {originalMemoryIndex + 14}";
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

                    codeBuilder.AppendLine(AddMemoryWrite($"{left} << {shiftRightOperandCast}{right}"));
                    codeBuilder.AppendLine(AddMemoryWrite($"{left} >> {shiftRightOperandCast}{right}"));

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
                        codeBuilder.AppendLine(Add8MemoryWrites(
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
                    var {variableName} = {(type != "int" ? $"({type})" : string.Empty)}input;");

                codeBuilder.AppendLine(AddMemoryWrite("~" + variableName));
                codeBuilder.AppendLine(AddMemoryWrite("+" + variableName));

                if (type != "ulong")
                {
                    codeBuilder.AppendLine(AddMemoryWrite("-" + variableName));
                }
            }

            System.IO.File.WriteAllText("AllUnaryOperatorExpressionVariations.cs", codeBuilder.ToString());


        }
    }
}
