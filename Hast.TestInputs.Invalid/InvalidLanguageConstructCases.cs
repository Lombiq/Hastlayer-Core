namespace Hast.TestInputs.Invalid
{
    public class InvalidLanguageConstructCases
    {
        public void CustomValueTypeReferenceEquals()
        {
            var x = ReferenceEquals(new CustomValueType { MyProperty = 1 }, new CustomValueType { MyProperty = 1 });
            var y = !x;
        }

        public void InvalidModelUsage()
        {
            var x = InvalidModel.StaticField;
            var y = -x;
        }

        private struct CustomValueType
        {
            public int MyProperty { get; set; }
        }
    }
}
