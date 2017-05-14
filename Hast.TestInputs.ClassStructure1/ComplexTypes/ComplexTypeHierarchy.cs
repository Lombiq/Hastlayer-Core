using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.ClassStructure1.ComplexTypes
{
    /// <summary>
    /// A type demonstrating a "complex" type hierarchy with base classes and interfaces.
    /// </summary>
    public class ComplexTypeHierarchy : BaseClass, IInterface1, IInterface2
    {
        // Explicit interface implementation.
        int IInterface1.Interface1Method1()
        {
            return PrivateMethod() + 3;
        }

        // Implicit interface implementation.
        public int Interface1Method2()
        {
            if (BaseClassMethod1())
            {
                return PrivateMethod() + StaticMethod();
            }
            else
            {
                return PrivateMethod();
            }
        }

        public int Interface2Method1()
        {
            return 10 + BaseInterfaceMethod2();
        }

        // Explicit interface implementation.
        int IBaseInterface.BaseInterfaceMethod1()
        {
            return 5;
        }

        public int BaseInterfaceMethod2()
        {
            return StaticMethod();
        }

        // A method that can't be a hardware interface since it's not a public virtual or an interface-declared method.
        public int NonVirtualNonInterfaceMehod()
        {
            return PrivateMethod();
        }

        // A generic method. Not yet supported.
        //public virtual void GenericMethod<T>(T input)
        //{
        //    var z = input;
        //    var y = z;
        //}


        private int PrivateMethod()
        {
            return 5 + StaticMethod();
        }

        // Method not referenced anywhere.
        private int UnusedMethod()
        {
            return 234;
        }


        private static int StaticMethod()
        {
            return 7;
        }
    }
}
