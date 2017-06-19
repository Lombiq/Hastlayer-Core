using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Various
{
    public class ObjectUsingCases
    {
        public void NullUsage()
        {
            var customObject = new MyClass { MyProperty = 5 };
            if (customObject == null)
            {
                customObject = new MyClass();
            }
            customObject = null;

            if (customObject != null)
            {
                customObject.MyProperty = 10;
            }
        }


        private class MyClass
        {
            public int MyProperty { get; set; }
        }
    }
}
