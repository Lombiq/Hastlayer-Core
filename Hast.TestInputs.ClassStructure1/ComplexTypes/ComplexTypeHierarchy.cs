namespace Hast.TestInputs.ClassStructure1.ComplexTypes
{
    /// <summary>
    /// A type demonstrating a "complex" type hierarchy with base classes and interfaces.
    /// </summary>
    public class ComplexTypeHierarchy : BaseClass, IInterface1, IInterface2
    {
        // Explicit interface implementation.
        void IInterface1.Interface1Method1()
        {
            PrivateMethod();
        }

        // Implicit interface implementation.
        public void Interface1Method2()
        {
            BaseClassMethod1();

            if (true)
            {
                PrivateMethod();
                StaticMethod();
            }
            else
            {
                PrivateMethod();
            }
        }

        public void Interface2Method1()
        {
            BaseInterfaceMethod2();
        }

        // Explicit interface implementation.
        void IBaseInterface.BaseInterfaceMethod1()
        {
            var x = 1;
        }

        public void BaseInterfaceMethod2()
        {
            StaticMethod();
        }

        // A method that can't be a hardware interface since it's not a public virtual or an interface-declared method.
        public void NonVirtualNonInterfaceMehod()
        {
            PrivateMethod();
        }

        // A generic method. Not yet supported.
        //public virtual void GenericMethod<T>(T input)
        //{
        //    var z = input;
        //    var y = z;
        //}


        private void PrivateMethod()
        {
            StaticMethod();
        }

        // Method not referenced anywhere.
        private void UnusedMethod()
        {
            var x = 1;
        }


        private static void StaticMethod()
        {
            var x = 1;
        }
    }
}
