﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Extensions
{
    public static class IntExtensions
    {
        public static Value ToVhdlValue(this int valueInt, DataType dataType)
        {
            return valueInt.ToString().ToVhdlValue(dataType);
        }
    }
}