﻿using System;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class SizedDataType : DataType
    {
        public int Size { get; set; }
        public IVhdlElement SizeExpression { get; set; }


        public SizedDataType(DataType baseType)
            : base(baseType)
        {
        }

        public SizedDataType(SizedDataType previous)
            : base(previous)
        {
            Size = previous.Size;
            SizeExpression = previous.SizeExpression;
        }

        public SizedDataType()
        {
        }


        public override DataType ToReference()
        {
            return this;
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

        public override bool Equals(object obj)
        {
            var otherType = obj as SizedDataType;
            if (otherType == null) return false;
            return base.Equals(obj) && 
                (SizeExpression == null ? Size == otherType.Size : SizeExpression.ToVhdl() == otherType.SizeExpression.ToVhdl());
        }
    }
}
