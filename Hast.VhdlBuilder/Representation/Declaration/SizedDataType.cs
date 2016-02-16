﻿using System;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class SizedDataType : DataType
    {
        public int Size { get; set; }
        public IVhdlElement SizeExpression { get; set; }


        public override string ToReferenceVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return ToVhdl(vhdlGenerationOptions);
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            if (Size == 0 && SizeExpression == null) return Name;

            if (Size != 0 && SizeExpression != null)
            {
                throw new InvalidOperationException("VHDL sized data types should have their size specified either as an integer value or as an expression, but not both.");
            }

            return
                Name +
                "(" +
                (Size != 0 ? (Size - 1).ToString() : SizeExpression.ToVhdl(vhdlGenerationOptions)) +
                " downto 0)";
        }
    }
}
