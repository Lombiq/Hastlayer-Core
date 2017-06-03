namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// Annotation for an array-typed <see cref="AstNode"/> that has a compile-time constant length.
    /// </summary>
    public class ConstantArrayLength
    {
        public int Length { get; private set; }


        public ConstantArrayLength(int length)
        {
            Length = length;
        }
    }
}
