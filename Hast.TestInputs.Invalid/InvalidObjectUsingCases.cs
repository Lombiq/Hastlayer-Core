using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Invalid
{
    public class InvalidObjectUsingCases
    {
        public void ReferenceAssignment(int input)
        {
            var customObject1 = new MyClass { MyProperty = input };
            var customObject2 = customObject1;
            customObject1.MyProperty += 1;
            customObject2.MyProperty += 1;
            // This is not allowed, since to achieve reference-like behavior we need to use VHDL aliases, but this
            // would also overwrite the original variable's value.
            customObject2 = new MyClass();
            customObject2.MyProperty += 1;
        }


        private class MyClass
        {
            public int MyProperty { get; set; }
        }
    }
}
