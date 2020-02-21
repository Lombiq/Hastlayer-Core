using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Static
{
    /// <summary>
    /// Test class to use all binary and unary operator expression variations. Needed to check whether proper type 
    /// conversions are implemented.
    /// </summary>
    public class BinaryAndUnaryOperatorExpressionCases
    {
        // Has an input to avoid results being statically evaluated by constant substitution.
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
        public void AllBinaryOperatorExpressionVariations(int input)
        {
            // You can generate all cases with the following code:

            //var needsShiftCastTypes = new[] { "uint", "long", "ulong" };
            //var types = new[] { "byte", "sbyte", "short", "ushort", "int" }.Union(needsShiftCastTypes).ToArray();
            //var codeBuilder = new StringBuilder();

            //for (int i = 0; i < types.Length; i++)
            //{
            //    var leftType = types[i];
            //    var left = leftType + "Left";

            //    //codeBuilder.AppendLine($@"
            //    //        {leftType} {left} = 123;");
            //    codeBuilder.AppendLine($@"
            //            {leftType} {left} = {(leftType != "int" ? $"({leftType})" : string.Empty)}input;");

            //    for (int j = 0; j < types.Length; j++)
            //    {
            //        var rightType = types[j];
            //        var right = rightType + "Right";

            //        if (i == 0)
            //        {
            //            codeBuilder.AppendLine($@"
            //            {rightType} {right} = {(rightType != "int" ? $"({rightType})" : string.Empty)}input;");
            //        }

            //        var shiftRightOperandCast = needsShiftCastTypes.Contains(rightType) ? "(int)" : string.Empty;

            //        var code = $@"
            //            var leftShift{i}{j} = {left} << {shiftRightOperandCast}{right};
            //            var rightShift{i}{j} = {left} >> {shiftRightOperandCast}{right};";

            //        // These variations can't be applied to the operands directly except for shift (which has a cast). 
            //        if (!(
            //            leftType == "sbyte" && rightType == "ulong" ||
            //            leftType == "short" && rightType == "ulong" ||
            //            leftType == "int" && rightType == "ulong" ||
            //            leftType == "long" && rightType == "ulong" ||
            //            leftType == "ulong" && rightType == "sbyte" ||
            //            leftType == "ulong" && rightType == "short" ||
            //            leftType == "ulong" && rightType == "int" ||
            //            leftType == "ulong" && rightType == "long"))
            //        {
            //            code += $@"
            //            var addition{i}{j} = {left} + {right};
            //            var subtraction{i}{j} = {left} - {right};
            //            var multiplication{i}{j} = {left} * {right};
            //            var division{i}{j} = {left} / {right};
            //            var remainder{i}{j} = {left} % {right};
            //            var bitwiseAnd{i}{j} = {left} & {right};
            //            var bitwiseOr{i}{j} = {left} | {right};
            //            var bitwiseXor{i}{j} = {left} ^ {right};";
            //        }

            //        codeBuilder.AppendLine(code);
            //    }
            //}

            //System.IO.File.WriteAllText("AllBinaryOperatorExpressionVariations.txt", codeBuilder.ToString());


            byte byteLeft = (byte)input;

            byte byteRight = (byte)input;

            var leftShift00 = byteLeft << byteRight;
            var rightShift00 = byteLeft >> byteRight;
            var addition00 = byteLeft + byteRight;
            var subtraction00 = byteLeft - byteRight;
            var multiplication00 = byteLeft * byteRight;
            var division00 = byteLeft / byteRight;
            var remainder00 = byteLeft % byteRight;
            var bitwiseAnd00 = byteLeft & byteRight;
            var bitwiseOr00 = byteLeft | byteRight;
            var bitwiseXor00 = byteLeft ^ byteRight;

            sbyte sbyteRight = (sbyte)input;

            var leftShift01 = byteLeft << sbyteRight;
            var rightShift01 = byteLeft >> sbyteRight;
            var addition01 = byteLeft + sbyteRight;
            var subtraction01 = byteLeft - sbyteRight;
            var multiplication01 = byteLeft * sbyteRight;
            var division01 = byteLeft / sbyteRight;
            var remainder01 = byteLeft % sbyteRight;
            var bitwiseAnd01 = byteLeft & sbyteRight;
            var bitwiseOr01 = byteLeft | sbyteRight;
            var bitwiseXor01 = byteLeft ^ sbyteRight;

            short shortRight = (short)input;

            var leftShift02 = byteLeft << shortRight;
            var rightShift02 = byteLeft >> shortRight;
            var addition02 = byteLeft + shortRight;
            var subtraction02 = byteLeft - shortRight;
            var multiplication02 = byteLeft * shortRight;
            var division02 = byteLeft / shortRight;
            var remainder02 = byteLeft % shortRight;
            var bitwiseAnd02 = byteLeft & shortRight;
            var bitwiseOr02 = byteLeft | shortRight;
            var bitwiseXor02 = byteLeft ^ shortRight;

            ushort ushortRight = (ushort)input;

            var leftShift03 = byteLeft << ushortRight;
            var rightShift03 = byteLeft >> ushortRight;
            var addition03 = byteLeft + ushortRight;
            var subtraction03 = byteLeft - ushortRight;
            var multiplication03 = byteLeft * ushortRight;
            var division03 = byteLeft / ushortRight;
            var remainder03 = byteLeft % ushortRight;
            var bitwiseAnd03 = byteLeft & ushortRight;
            var bitwiseOr03 = byteLeft | ushortRight;
            var bitwiseXor03 = byteLeft ^ ushortRight;

            int intRight = input;

            var leftShift04 = byteLeft << intRight;
            var rightShift04 = byteLeft >> intRight;
            var addition04 = byteLeft + intRight;
            var subtraction04 = byteLeft - intRight;
            var multiplication04 = byteLeft * intRight;
            var division04 = byteLeft / intRight;
            var remainder04 = byteLeft % intRight;
            var bitwiseAnd04 = byteLeft & intRight;
            var bitwiseOr04 = byteLeft | intRight;
            var bitwiseXor04 = byteLeft ^ intRight;

            uint uintRight = (uint)input;

            var leftShift05 = byteLeft << (int)uintRight;
            var rightShift05 = byteLeft >> (int)uintRight;
            var addition05 = byteLeft + uintRight;
            var subtraction05 = byteLeft - uintRight;
            var multiplication05 = byteLeft * uintRight;
            var division05 = byteLeft / uintRight;
            var remainder05 = byteLeft % uintRight;
            var bitwiseAnd05 = byteLeft & uintRight;
            var bitwiseOr05 = byteLeft | uintRight;
            var bitwiseXor05 = byteLeft ^ uintRight;

            long longRight = (long)input;

            var leftShift06 = byteLeft << (int)longRight;
            var rightShift06 = byteLeft >> (int)longRight;
            var addition06 = byteLeft + longRight;
            var subtraction06 = byteLeft - longRight;
            var multiplication06 = byteLeft * longRight;
            var division06 = byteLeft / longRight;
            var remainder06 = byteLeft % longRight;
            var bitwiseAnd06 = byteLeft & longRight;
            var bitwiseOr06 = byteLeft | longRight;
            var bitwiseXor06 = byteLeft ^ longRight;

            ulong ulongRight = (ulong)input;

            var leftShift07 = byteLeft << (int)ulongRight;
            var rightShift07 = byteLeft >> (int)ulongRight;
            var addition07 = byteLeft + ulongRight;
            var subtraction07 = byteLeft - ulongRight;
            var multiplication07 = byteLeft * ulongRight;
            var division07 = byteLeft / ulongRight;
            var remainder07 = byteLeft % ulongRight;
            var bitwiseAnd07 = byteLeft & ulongRight;
            var bitwiseOr07 = byteLeft | ulongRight;
            var bitwiseXor07 = byteLeft ^ ulongRight;

            sbyte sbyteLeft = (sbyte)input;

            var leftShift10 = sbyteLeft << byteRight;
            var rightShift10 = sbyteLeft >> byteRight;
            var addition10 = sbyteLeft + byteRight;
            var subtraction10 = sbyteLeft - byteRight;
            var multiplication10 = sbyteLeft * byteRight;
            var division10 = sbyteLeft / byteRight;
            var remainder10 = sbyteLeft % byteRight;
            var bitwiseAnd10 = sbyteLeft & byteRight;
            var bitwiseOr10 = sbyteLeft | byteRight;
            var bitwiseXor10 = sbyteLeft ^ byteRight;

            var leftShift11 = sbyteLeft << sbyteRight;
            var rightShift11 = sbyteLeft >> sbyteRight;
            var addition11 = sbyteLeft + sbyteRight;
            var subtraction11 = sbyteLeft - sbyteRight;
            var multiplication11 = sbyteLeft * sbyteRight;
            var division11 = sbyteLeft / sbyteRight;
            var remainder11 = sbyteLeft % sbyteRight;
            var bitwiseAnd11 = sbyteLeft & sbyteRight;
            var bitwiseOr11 = sbyteLeft | sbyteRight;
            var bitwiseXor11 = sbyteLeft ^ sbyteRight;

            var leftShift12 = sbyteLeft << shortRight;
            var rightShift12 = sbyteLeft >> shortRight;
            var addition12 = sbyteLeft + shortRight;
            var subtraction12 = sbyteLeft - shortRight;
            var multiplication12 = sbyteLeft * shortRight;
            var division12 = sbyteLeft / shortRight;
            var remainder12 = sbyteLeft % shortRight;
            var bitwiseAnd12 = sbyteLeft & shortRight;
            var bitwiseOr12 = sbyteLeft | shortRight;
            var bitwiseXor12 = sbyteLeft ^ shortRight;

            var leftShift13 = sbyteLeft << ushortRight;
            var rightShift13 = sbyteLeft >> ushortRight;
            var addition13 = sbyteLeft + ushortRight;
            var subtraction13 = sbyteLeft - ushortRight;
            var multiplication13 = sbyteLeft * ushortRight;
            var division13 = sbyteLeft / ushortRight;
            var remainder13 = sbyteLeft % ushortRight;
            var bitwiseAnd13 = sbyteLeft & ushortRight;
            var bitwiseOr13 = sbyteLeft | ushortRight;
            var bitwiseXor13 = sbyteLeft ^ ushortRight;

            var leftShift14 = sbyteLeft << intRight;
            var rightShift14 = sbyteLeft >> intRight;
            var addition14 = sbyteLeft + intRight;
            var subtraction14 = sbyteLeft - intRight;
            var multiplication14 = sbyteLeft * intRight;
            var division14 = sbyteLeft / intRight;
            var remainder14 = sbyteLeft % intRight;
            var bitwiseAnd14 = sbyteLeft & intRight;
            var bitwiseOr14 = sbyteLeft | intRight;
            var bitwiseXor14 = sbyteLeft ^ intRight;

            var leftShift15 = sbyteLeft << (int)uintRight;
            var rightShift15 = sbyteLeft >> (int)uintRight;
            var addition15 = sbyteLeft + uintRight;
            var subtraction15 = sbyteLeft - uintRight;
            var multiplication15 = sbyteLeft * uintRight;
            var division15 = sbyteLeft / uintRight;
            var remainder15 = sbyteLeft % uintRight;
            var bitwiseAnd15 = sbyteLeft & uintRight;
            var bitwiseOr15 = sbyteLeft | uintRight;
            var bitwiseXor15 = sbyteLeft ^ uintRight;

            var leftShift16 = sbyteLeft << (int)longRight;
            var rightShift16 = sbyteLeft >> (int)longRight;
            var addition16 = sbyteLeft + longRight;
            var subtraction16 = sbyteLeft - longRight;
            var multiplication16 = sbyteLeft * longRight;
            var division16 = sbyteLeft / longRight;
            var remainder16 = sbyteLeft % longRight;
            var bitwiseAnd16 = sbyteLeft & longRight;
            var bitwiseOr16 = sbyteLeft | longRight;
            var bitwiseXor16 = sbyteLeft ^ longRight;

            var leftShift17 = sbyteLeft << (int)ulongRight;
            var rightShift17 = sbyteLeft >> (int)ulongRight;

            short shortLeft = (short)input;

            var leftShift20 = shortLeft << byteRight;
            var rightShift20 = shortLeft >> byteRight;
            var addition20 = shortLeft + byteRight;
            var subtraction20 = shortLeft - byteRight;
            var multiplication20 = shortLeft * byteRight;
            var division20 = shortLeft / byteRight;
            var remainder20 = shortLeft % byteRight;
            var bitwiseAnd20 = shortLeft & byteRight;
            var bitwiseOr20 = shortLeft | byteRight;
            var bitwiseXor20 = shortLeft ^ byteRight;

            var leftShift21 = shortLeft << sbyteRight;
            var rightShift21 = shortLeft >> sbyteRight;
            var addition21 = shortLeft + sbyteRight;
            var subtraction21 = shortLeft - sbyteRight;
            var multiplication21 = shortLeft * sbyteRight;
            var division21 = shortLeft / sbyteRight;
            var remainder21 = shortLeft % sbyteRight;
            var bitwiseAnd21 = shortLeft & sbyteRight;
            var bitwiseOr21 = shortLeft | sbyteRight;
            var bitwiseXor21 = shortLeft ^ sbyteRight;

            var leftShift22 = shortLeft << shortRight;
            var rightShift22 = shortLeft >> shortRight;
            var addition22 = shortLeft + shortRight;
            var subtraction22 = shortLeft - shortRight;
            var multiplication22 = shortLeft * shortRight;
            var division22 = shortLeft / shortRight;
            var remainder22 = shortLeft % shortRight;
            var bitwiseAnd22 = shortLeft & shortRight;
            var bitwiseOr22 = shortLeft | shortRight;
            var bitwiseXor22 = shortLeft ^ shortRight;

            var leftShift23 = shortLeft << ushortRight;
            var rightShift23 = shortLeft >> ushortRight;
            var addition23 = shortLeft + ushortRight;
            var subtraction23 = shortLeft - ushortRight;
            var multiplication23 = shortLeft * ushortRight;
            var division23 = shortLeft / ushortRight;
            var remainder23 = shortLeft % ushortRight;
            var bitwiseAnd23 = shortLeft & ushortRight;
            var bitwiseOr23 = shortLeft | ushortRight;
            var bitwiseXor23 = shortLeft ^ ushortRight;

            var leftShift24 = shortLeft << intRight;
            var rightShift24 = shortLeft >> intRight;
            var addition24 = shortLeft + intRight;
            var subtraction24 = shortLeft - intRight;
            var multiplication24 = shortLeft * intRight;
            var division24 = shortLeft / intRight;
            var remainder24 = shortLeft % intRight;
            var bitwiseAnd24 = shortLeft & intRight;
            var bitwiseOr24 = shortLeft | intRight;
            var bitwiseXor24 = shortLeft ^ intRight;

            var leftShift25 = shortLeft << (int)uintRight;
            var rightShift25 = shortLeft >> (int)uintRight;
            var addition25 = shortLeft + uintRight;
            var subtraction25 = shortLeft - uintRight;
            var multiplication25 = shortLeft * uintRight;
            var division25 = shortLeft / uintRight;
            var remainder25 = shortLeft % uintRight;
            var bitwiseAnd25 = shortLeft & uintRight;
            var bitwiseOr25 = shortLeft | uintRight;
            var bitwiseXor25 = shortLeft ^ uintRight;

            var leftShift26 = shortLeft << (int)longRight;
            var rightShift26 = shortLeft >> (int)longRight;
            var addition26 = shortLeft + longRight;
            var subtraction26 = shortLeft - longRight;
            var multiplication26 = shortLeft * longRight;
            var division26 = shortLeft / longRight;
            var remainder26 = shortLeft % longRight;
            var bitwiseAnd26 = shortLeft & longRight;
            var bitwiseOr26 = shortLeft | longRight;
            var bitwiseXor26 = shortLeft ^ longRight;

            var leftShift27 = shortLeft << (int)ulongRight;
            var rightShift27 = shortLeft >> (int)ulongRight;

            ushort ushortLeft = (ushort)input;

            var leftShift30 = ushortLeft << byteRight;
            var rightShift30 = ushortLeft >> byteRight;
            var addition30 = ushortLeft + byteRight;
            var subtraction30 = ushortLeft - byteRight;
            var multiplication30 = ushortLeft * byteRight;
            var division30 = ushortLeft / byteRight;
            var remainder30 = ushortLeft % byteRight;
            var bitwiseAnd30 = ushortLeft & byteRight;
            var bitwiseOr30 = ushortLeft | byteRight;
            var bitwiseXor30 = ushortLeft ^ byteRight;

            var leftShift31 = ushortLeft << sbyteRight;
            var rightShift31 = ushortLeft >> sbyteRight;
            var addition31 = ushortLeft + sbyteRight;
            var subtraction31 = ushortLeft - sbyteRight;
            var multiplication31 = ushortLeft * sbyteRight;
            var division31 = ushortLeft / sbyteRight;
            var remainder31 = ushortLeft % sbyteRight;
            var bitwiseAnd31 = ushortLeft & sbyteRight;
            var bitwiseOr31 = ushortLeft | sbyteRight;
            var bitwiseXor31 = ushortLeft ^ sbyteRight;

            var leftShift32 = ushortLeft << shortRight;
            var rightShift32 = ushortLeft >> shortRight;
            var addition32 = ushortLeft + shortRight;
            var subtraction32 = ushortLeft - shortRight;
            var multiplication32 = ushortLeft * shortRight;
            var division32 = ushortLeft / shortRight;
            var remainder32 = ushortLeft % shortRight;
            var bitwiseAnd32 = ushortLeft & shortRight;
            var bitwiseOr32 = ushortLeft | shortRight;
            var bitwiseXor32 = ushortLeft ^ shortRight;

            var leftShift33 = ushortLeft << ushortRight;
            var rightShift33 = ushortLeft >> ushortRight;
            var addition33 = ushortLeft + ushortRight;
            var subtraction33 = ushortLeft - ushortRight;
            var multiplication33 = ushortLeft * ushortRight;
            var division33 = ushortLeft / ushortRight;
            var remainder33 = ushortLeft % ushortRight;
            var bitwiseAnd33 = ushortLeft & ushortRight;
            var bitwiseOr33 = ushortLeft | ushortRight;
            var bitwiseXor33 = ushortLeft ^ ushortRight;

            var leftShift34 = ushortLeft << intRight;
            var rightShift34 = ushortLeft >> intRight;
            var addition34 = ushortLeft + intRight;
            var subtraction34 = ushortLeft - intRight;
            var multiplication34 = ushortLeft * intRight;
            var division34 = ushortLeft / intRight;
            var remainder34 = ushortLeft % intRight;
            var bitwiseAnd34 = ushortLeft & intRight;
            var bitwiseOr34 = ushortLeft | intRight;
            var bitwiseXor34 = ushortLeft ^ intRight;

            var leftShift35 = ushortLeft << (int)uintRight;
            var rightShift35 = ushortLeft >> (int)uintRight;
            var addition35 = ushortLeft + uintRight;
            var subtraction35 = ushortLeft - uintRight;
            var multiplication35 = ushortLeft * uintRight;
            var division35 = ushortLeft / uintRight;
            var remainder35 = ushortLeft % uintRight;
            var bitwiseAnd35 = ushortLeft & uintRight;
            var bitwiseOr35 = ushortLeft | uintRight;
            var bitwiseXor35 = ushortLeft ^ uintRight;

            var leftShift36 = ushortLeft << (int)longRight;
            var rightShift36 = ushortLeft >> (int)longRight;
            var addition36 = ushortLeft + longRight;
            var subtraction36 = ushortLeft - longRight;
            var multiplication36 = ushortLeft * longRight;
            var division36 = ushortLeft / longRight;
            var remainder36 = ushortLeft % longRight;
            var bitwiseAnd36 = ushortLeft & longRight;
            var bitwiseOr36 = ushortLeft | longRight;
            var bitwiseXor36 = ushortLeft ^ longRight;

            var leftShift37 = ushortLeft << (int)ulongRight;
            var rightShift37 = ushortLeft >> (int)ulongRight;
            var addition37 = ushortLeft + ulongRight;
            var subtraction37 = ushortLeft - ulongRight;
            var multiplication37 = ushortLeft * ulongRight;
            var division37 = ushortLeft / ulongRight;
            var remainder37 = ushortLeft % ulongRight;
            var bitwiseAnd37 = ushortLeft & ulongRight;
            var bitwiseOr37 = ushortLeft | ulongRight;
            var bitwiseXor37 = ushortLeft ^ ulongRight;

            int intLeft = input;

            var leftShift40 = intLeft << byteRight;
            var rightShift40 = intLeft >> byteRight;
            var addition40 = intLeft + byteRight;
            var subtraction40 = intLeft - byteRight;
            var multiplication40 = intLeft * byteRight;
            var division40 = intLeft / byteRight;
            var remainder40 = intLeft % byteRight;
            var bitwiseAnd40 = intLeft & byteRight;
            var bitwiseOr40 = intLeft | byteRight;
            var bitwiseXor40 = intLeft ^ byteRight;

            var leftShift41 = intLeft << sbyteRight;
            var rightShift41 = intLeft >> sbyteRight;
            var addition41 = intLeft + sbyteRight;
            var subtraction41 = intLeft - sbyteRight;
            var multiplication41 = intLeft * sbyteRight;
            var division41 = intLeft / sbyteRight;
            var remainder41 = intLeft % sbyteRight;
            var bitwiseAnd41 = intLeft & sbyteRight;
            var bitwiseOr41 = intLeft | sbyteRight;
            var bitwiseXor41 = intLeft ^ sbyteRight;

            var leftShift42 = intLeft << shortRight;
            var rightShift42 = intLeft >> shortRight;
            var addition42 = intLeft + shortRight;
            var subtraction42 = intLeft - shortRight;
            var multiplication42 = intLeft * shortRight;
            var division42 = intLeft / shortRight;
            var remainder42 = intLeft % shortRight;
            var bitwiseAnd42 = intLeft & shortRight;
            var bitwiseOr42 = intLeft | shortRight;
            var bitwiseXor42 = intLeft ^ shortRight;

            var leftShift43 = intLeft << ushortRight;
            var rightShift43 = intLeft >> ushortRight;
            var addition43 = intLeft + ushortRight;
            var subtraction43 = intLeft - ushortRight;
            var multiplication43 = intLeft * ushortRight;
            var division43 = intLeft / ushortRight;
            var remainder43 = intLeft % ushortRight;
            var bitwiseAnd43 = intLeft & ushortRight;
            var bitwiseOr43 = intLeft | ushortRight;
            var bitwiseXor43 = intLeft ^ ushortRight;

            var leftShift44 = intLeft << intRight;
            var rightShift44 = intLeft >> intRight;
            var addition44 = intLeft + intRight;
            var subtraction44 = intLeft - intRight;
            var multiplication44 = intLeft * intRight;
            var division44 = intLeft / intRight;
            var remainder44 = intLeft % intRight;
            var bitwiseAnd44 = intLeft & intRight;
            var bitwiseOr44 = intLeft | intRight;
            var bitwiseXor44 = intLeft ^ intRight;

            var leftShift45 = intLeft << (int)uintRight;
            var rightShift45 = intLeft >> (int)uintRight;
            var addition45 = intLeft + uintRight;
            var subtraction45 = intLeft - uintRight;
            var multiplication45 = intLeft * uintRight;
            var division45 = intLeft / uintRight;
            var remainder45 = intLeft % uintRight;
            var bitwiseAnd45 = intLeft & uintRight;
            var bitwiseOr45 = intLeft | uintRight;
            var bitwiseXor45 = intLeft ^ uintRight;

            var leftShift46 = intLeft << (int)longRight;
            var rightShift46 = intLeft >> (int)longRight;
            var addition46 = intLeft + longRight;
            var subtraction46 = intLeft - longRight;
            var multiplication46 = intLeft * longRight;
            var division46 = intLeft / longRight;
            var remainder46 = intLeft % longRight;
            var bitwiseAnd46 = intLeft & longRight;
            var bitwiseOr46 = intLeft | longRight;
            var bitwiseXor46 = intLeft ^ longRight;

            var leftShift47 = intLeft << (int)ulongRight;
            var rightShift47 = intLeft >> (int)ulongRight;

            uint uintLeft = (uint)input;

            var leftShift50 = uintLeft << byteRight;
            var rightShift50 = uintLeft >> byteRight;
            var addition50 = uintLeft + byteRight;
            var subtraction50 = uintLeft - byteRight;
            var multiplication50 = uintLeft * byteRight;
            var division50 = uintLeft / byteRight;
            var remainder50 = uintLeft % byteRight;
            var bitwiseAnd50 = uintLeft & byteRight;
            var bitwiseOr50 = uintLeft | byteRight;
            var bitwiseXor50 = uintLeft ^ byteRight;

            var leftShift51 = uintLeft << sbyteRight;
            var rightShift51 = uintLeft >> sbyteRight;
            var addition51 = uintLeft + sbyteRight;
            var subtraction51 = uintLeft - sbyteRight;
            var multiplication51 = uintLeft * sbyteRight;
            var division51 = uintLeft / sbyteRight;
            var remainder51 = uintLeft % sbyteRight;
            var bitwiseAnd51 = uintLeft & sbyteRight;
            var bitwiseOr51 = uintLeft | sbyteRight;
            var bitwiseXor51 = uintLeft ^ sbyteRight;

            var leftShift52 = uintLeft << shortRight;
            var rightShift52 = uintLeft >> shortRight;
            var addition52 = uintLeft + shortRight;
            var subtraction52 = uintLeft - shortRight;
            var multiplication52 = uintLeft * shortRight;
            var division52 = uintLeft / shortRight;
            var remainder52 = uintLeft % shortRight;
            var bitwiseAnd52 = uintLeft & shortRight;
            var bitwiseOr52 = uintLeft | shortRight;
            var bitwiseXor52 = uintLeft ^ shortRight;

            var leftShift53 = uintLeft << ushortRight;
            var rightShift53 = uintLeft >> ushortRight;
            var addition53 = uintLeft + ushortRight;
            var subtraction53 = uintLeft - ushortRight;
            var multiplication53 = uintLeft * ushortRight;
            var division53 = uintLeft / ushortRight;
            var remainder53 = uintLeft % ushortRight;
            var bitwiseAnd53 = uintLeft & ushortRight;
            var bitwiseOr53 = uintLeft | ushortRight;
            var bitwiseXor53 = uintLeft ^ ushortRight;

            var leftShift54 = uintLeft << intRight;
            var rightShift54 = uintLeft >> intRight;
            var addition54 = uintLeft + intRight;
            var subtraction54 = uintLeft - intRight;
            var multiplication54 = uintLeft * intRight;
            var division54 = uintLeft / intRight;
            var remainder54 = uintLeft % intRight;
            var bitwiseAnd54 = uintLeft & intRight;
            var bitwiseOr54 = uintLeft | intRight;
            var bitwiseXor54 = uintLeft ^ intRight;

            var leftShift55 = uintLeft << (int)uintRight;
            var rightShift55 = uintLeft >> (int)uintRight;
            var addition55 = uintLeft + uintRight;
            var subtraction55 = uintLeft - uintRight;
            var multiplication55 = uintLeft * uintRight;
            var division55 = uintLeft / uintRight;
            var remainder55 = uintLeft % uintRight;
            var bitwiseAnd55 = uintLeft & uintRight;
            var bitwiseOr55 = uintLeft | uintRight;
            var bitwiseXor55 = uintLeft ^ uintRight;

            var leftShift56 = uintLeft << (int)longRight;
            var rightShift56 = uintLeft >> (int)longRight;
            var addition56 = uintLeft + longRight;
            var subtraction56 = uintLeft - longRight;
            var multiplication56 = uintLeft * longRight;
            var division56 = uintLeft / longRight;
            var remainder56 = uintLeft % longRight;
            var bitwiseAnd56 = uintLeft & longRight;
            var bitwiseOr56 = uintLeft | longRight;
            var bitwiseXor56 = uintLeft ^ longRight;

            var leftShift57 = uintLeft << (int)ulongRight;
            var rightShift57 = uintLeft >> (int)ulongRight;
            var addition57 = uintLeft + ulongRight;
            var subtraction57 = uintLeft - ulongRight;
            var multiplication57 = uintLeft * ulongRight;
            var division57 = uintLeft / ulongRight;
            var remainder57 = uintLeft % ulongRight;
            var bitwiseAnd57 = uintLeft & ulongRight;
            var bitwiseOr57 = uintLeft | ulongRight;
            var bitwiseXor57 = uintLeft ^ ulongRight;

            long longLeft = (long)input;

            var leftShift60 = longLeft << byteRight;
            var rightShift60 = longLeft >> byteRight;
            var addition60 = longLeft + byteRight;
            var subtraction60 = longLeft - byteRight;
            var multiplication60 = longLeft * byteRight;
            var division60 = longLeft / byteRight;
            var remainder60 = longLeft % byteRight;
            var bitwiseAnd60 = longLeft & byteRight;
            var bitwiseOr60 = longLeft | byteRight;
            var bitwiseXor60 = longLeft ^ byteRight;

            var leftShift61 = longLeft << sbyteRight;
            var rightShift61 = longLeft >> sbyteRight;
            var addition61 = longLeft + sbyteRight;
            var subtraction61 = longLeft - sbyteRight;
            var multiplication61 = longLeft * sbyteRight;
            var division61 = longLeft / sbyteRight;
            var remainder61 = longLeft % sbyteRight;
            var bitwiseAnd61 = longLeft & sbyteRight;
            var bitwiseOr61 = longLeft | sbyteRight;
            var bitwiseXor61 = longLeft ^ sbyteRight;

            var leftShift62 = longLeft << shortRight;
            var rightShift62 = longLeft >> shortRight;
            var addition62 = longLeft + shortRight;
            var subtraction62 = longLeft - shortRight;
            var multiplication62 = longLeft * shortRight;
            var division62 = longLeft / shortRight;
            var remainder62 = longLeft % shortRight;
            var bitwiseAnd62 = longLeft & shortRight;
            var bitwiseOr62 = longLeft | shortRight;
            var bitwiseXor62 = longLeft ^ shortRight;

            var leftShift63 = longLeft << ushortRight;
            var rightShift63 = longLeft >> ushortRight;
            var addition63 = longLeft + ushortRight;
            var subtraction63 = longLeft - ushortRight;
            var multiplication63 = longLeft * ushortRight;
            var division63 = longLeft / ushortRight;
            var remainder63 = longLeft % ushortRight;
            var bitwiseAnd63 = longLeft & ushortRight;
            var bitwiseOr63 = longLeft | ushortRight;
            var bitwiseXor63 = longLeft ^ ushortRight;

            var leftShift64 = longLeft << intRight;
            var rightShift64 = longLeft >> intRight;
            var addition64 = longLeft + intRight;
            var subtraction64 = longLeft - intRight;
            var multiplication64 = longLeft * intRight;
            var division64 = longLeft / intRight;
            var remainder64 = longLeft % intRight;
            var bitwiseAnd64 = longLeft & intRight;
            var bitwiseOr64 = longLeft | intRight;
            var bitwiseXor64 = longLeft ^ intRight;

            var leftShift65 = longLeft << (int)uintRight;
            var rightShift65 = longLeft >> (int)uintRight;
            var addition65 = longLeft + uintRight;
            var subtraction65 = longLeft - uintRight;
            var multiplication65 = longLeft * uintRight;
            var division65 = longLeft / uintRight;
            var remainder65 = longLeft % uintRight;
            var bitwiseAnd65 = longLeft & uintRight;
            var bitwiseOr65 = longLeft | uintRight;
            var bitwiseXor65 = longLeft ^ uintRight;

            var leftShift66 = longLeft << (int)longRight;
            var rightShift66 = longLeft >> (int)longRight;
            var addition66 = longLeft + longRight;
            var subtraction66 = longLeft - longRight;
            var multiplication66 = longLeft * longRight;
            var division66 = longLeft / longRight;
            var remainder66 = longLeft % longRight;
            var bitwiseAnd66 = longLeft & longRight;
            var bitwiseOr66 = longLeft | longRight;
            var bitwiseXor66 = longLeft ^ longRight;

            var leftShift67 = longLeft << (int)ulongRight;
            var rightShift67 = longLeft >> (int)ulongRight;

            ulong ulongLeft = (ulong)input;

            var leftShift70 = ulongLeft << byteRight;
            var rightShift70 = ulongLeft >> byteRight;
            var addition70 = ulongLeft + byteRight;
            var subtraction70 = ulongLeft - byteRight;
            var multiplication70 = ulongLeft * byteRight;
            var division70 = ulongLeft / byteRight;
            var remainder70 = ulongLeft % byteRight;
            var bitwiseAnd70 = ulongLeft & byteRight;
            var bitwiseOr70 = ulongLeft | byteRight;
            var bitwiseXor70 = ulongLeft ^ byteRight;

            var leftShift71 = ulongLeft << sbyteRight;
            var rightShift71 = ulongLeft >> sbyteRight;

            var leftShift72 = ulongLeft << shortRight;
            var rightShift72 = ulongLeft >> shortRight;

            var leftShift73 = ulongLeft << ushortRight;
            var rightShift73 = ulongLeft >> ushortRight;
            var addition73 = ulongLeft + ushortRight;
            var subtraction73 = ulongLeft - ushortRight;
            var multiplication73 = ulongLeft * ushortRight;
            var division73 = ulongLeft / ushortRight;
            var remainder73 = ulongLeft % ushortRight;
            var bitwiseAnd73 = ulongLeft & ushortRight;
            var bitwiseOr73 = ulongLeft | ushortRight;
            var bitwiseXor73 = ulongLeft ^ ushortRight;

            var leftShift74 = ulongLeft << intRight;
            var rightShift74 = ulongLeft >> intRight;

            var leftShift75 = ulongLeft << (int)uintRight;
            var rightShift75 = ulongLeft >> (int)uintRight;
            var addition75 = ulongLeft + uintRight;
            var subtraction75 = ulongLeft - uintRight;
            var multiplication75 = ulongLeft * uintRight;
            var division75 = ulongLeft / uintRight;
            var remainder75 = ulongLeft % uintRight;
            var bitwiseAnd75 = ulongLeft & uintRight;
            var bitwiseOr75 = ulongLeft | uintRight;
            var bitwiseXor75 = ulongLeft ^ uintRight;

            var leftShift76 = ulongLeft << (int)longRight;
            var rightShift76 = ulongLeft >> (int)longRight;

            var leftShift77 = ulongLeft << (int)ulongRight;
            var rightShift77 = ulongLeft >> (int)ulongRight;
            var addition77 = ulongLeft + ulongRight;
            var subtraction77 = ulongLeft - ulongRight;
            var multiplication77 = ulongLeft * ulongRight;
            var division77 = ulongLeft / ulongRight;
            var remainder77 = ulongLeft % ulongRight;
            var bitwiseAnd77 = ulongLeft & ulongRight;
            var bitwiseOr77 = ulongLeft | ulongRight;
            var bitwiseXor77 = ulongLeft ^ ulongRight;

        }
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

        // Has an input to avoid results being statically evaluated by constant substitution.
        public void AllUnaryOperatorExpressionVariations(int input)
        {
            var byteOperand = (byte)input;
            var unaryPlus1 = +byteOperand;
            var unaryMinus1 = -byteOperand;
            var bitwiseComplemen1 = ~byteOperand;

            var sbyteOperand = (sbyte)input;
            var unaryPlus2 = +sbyteOperand;
            var unaryMinus2 = -sbyteOperand;
            var bitwiseComplement2 = ~sbyteOperand;

            var shortOperand = (short)input;
            var unaryPlus3 = +shortOperand;
            var unaryMinus3 = -shortOperand;
            var bitwiseComplement3 = ~shortOperand;

            var ushortOperand = (ushort)input;
            var unaryPlus4 = +ushortOperand;
            var unaryMinus4 = -ushortOperand;
            var bitwiseComplement4 = ~ushortOperand;

            var intOperand = input;
            var unaryPlus5 = +intOperand;
            var unaryMinus5 = -intOperand;
            var bitwiseComplement5 = ~intOperand;

            var uintOperand = (uint)input;
            var unaryPlus6 = +uintOperand;
            var unaryMinus6 = -uintOperand;
            var bitwiseComplement6 = ~uintOperand;

            var longOperand = (long)input;
            var unaryPlus7 = +longOperand;
            var unaryMinus7 = -longOperand;
            var bitwiseComplement7 = ~longOperand;

            // Ulong can't be negated.
            var ulongOperand = (ulong)input;
            var unaryPlus8 = +ulongOperand;
            var bitwiseComplement8 = ~ulongOperand;

            // Not supported.
            //var charOperand = (char)input;
            //var unaryPlu97 = +charOperand;
            //var unaryMinus9 = -charOperand;
            //var bitwiseComplement9 = ~charOperand;
        }
    }
}
