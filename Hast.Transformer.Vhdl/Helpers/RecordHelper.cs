﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class RecordHelper
    {
        public static NullableRecord CreateNullableRecord(string name, IEnumerable<RecordField> fields)
        {
            var record = new NullableRecord { Name = name };
            record.Fields.AddRange(fields);
            return record;
        }

        //public static Record CreateArrayHoldingRecord(ArrayTypeBase arrayType) =>
        //    CreateNullableRecord()
    }
}