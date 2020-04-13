namespace Hast.TestInputs.Static
{
    public class OptionaParametersCases
    {
        public void OmittedOptionalParameters(int input)
        {
            var a = new MyClass(input);
            a.Method(input);
        }


        private class MyClass
        {
            private int _state;


            public MyClass(int input, int add = 10)
            {
                _state = input + add;
            }


            public void Method(int input, int add = 11)
            {
                _state = input + add;
            }
        }
    }
}
