namespace Hast.TestInputs.Invalid
{
    public class InvalidLanguageConstructCases
    {
        public void BreakStatements()
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) break;
            }
        }

        public void CustomValueTypeReferenceEquals()
        {
            var x = ReferenceEquals(new CustomValueType { MyProperty = 1 }, new CustomValueType { MyProperty = 1 });
            var y = !x;
        }


        private struct CustomValueType
        {
            public int MyProperty { get; set; }
        }
    }
}
