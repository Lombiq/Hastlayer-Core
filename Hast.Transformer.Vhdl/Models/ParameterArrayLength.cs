namespace Hast.Transformer.Vhdl.Models
{
    public class ParameterArrayLength
    {
        public int Length { get; private set; }


        public ParameterArrayLength(int length)
        {
            Length = length;
        }
    }
}
