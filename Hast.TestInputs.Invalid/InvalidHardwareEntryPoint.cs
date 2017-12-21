namespace Hast.TestInputs.Invalid
{
    public class InvalidHardwareEntryPoint
    {
        public int MyProperty { get; set; }
        private int _myField = 0;


        public InvalidHardwareEntryPoint()
        {
            var x = 4;
            var y = x + 3;
        }


        public virtual void EntryPointMethod()
        {
            var x = MyProperty + _myField;
            var y = x + 3;
        }
    }
}
