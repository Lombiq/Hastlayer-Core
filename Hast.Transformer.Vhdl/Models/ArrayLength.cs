namespace Hast.Transformer.Vhdl.Models
{
    public class ArrayLength
    {
        public int Length { get; private set; }


        public ArrayLength(int length)
        {
            Length = length;
        }
    }
}
