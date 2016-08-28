using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Record : DataType
    {
        public List<RecordField> Fields { get; set; }


        public Record()
        {
            Fields = new List<RecordField>();
            TypeCategory = DataTypeCategory.Composite;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "type " + vhdlGenerationOptions.ShortenName(Name) + " is record " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Fields.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end record", vhdlGenerationOptions);
        }
    }


    public class RecordField : TypedDataObjectBase
    {
        public RecordField()
        {
            DataObjectKind = DataObjectKind.Variable;
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                vhdlGenerationOptions.ShortenName(Name) +
                (DataType != null ? ": " + DataType.ToReference().ToVhdl(vhdlGenerationOptions) : string.Empty),
                vhdlGenerationOptions);
        }
    }
}
