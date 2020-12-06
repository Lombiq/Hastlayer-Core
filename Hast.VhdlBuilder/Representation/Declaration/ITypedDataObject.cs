namespace Hast.VhdlBuilder.Representation.Declaration
{
    public interface ITypedDataObject : IDataObject
    {
        DataType DataType { get; set; }
    }
}
