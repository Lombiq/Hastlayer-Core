using System.Diagnostics.CodeAnalysis;

namespace Hast.TestInputs.ClassStructure1.ComplexTypes
{
    /// <summary>
    /// A type demonstrating a "complex" type hierarchy with base classes and interfaces.
    /// </summary>
    // Class inheritance is not yet supported.
    public class ComplexTypeHierarchy : /*BaseClass,*/ IInterface1, IInterface2
    {
        private const string Nope = "This is the point of the exercise.";

        // Explicit interface implementation.
        [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "I'd break the thest.")]
        void IInterface1.Interface1Method1() => PrivateMethod();

        // Implicit interface implementation.
        public void Interface1Method2()
        {
            //// var x = BaseClassMethod1(4);
            //// var y = x + 4;
            //// var z = x + y;

            PrivateMethod();
            StaticMethod();
        }

        public void Interface2Method1() => BaseInterfaceMethod2();

        // Explicit interface implementation.
        [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = Nope)]
        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = Nope)]
        [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = Nope)]
        void IBaseInterface.BaseInterfaceMethod1()
        {
            // This is the point of the exercise.
            var x = 1;
        }

        public void BaseInterfaceMethod2() => StaticMethod();

        // A method that can't be a hardware interface since it's not a public virtual or an interface-declared method.
        public void NonVirtualNonInterfaceMehod() => PrivateMethod();

        ////  A generic method. Not yet supported.
        //// public virtual void GenericMethod<T>(T input)
        //// {
        ////     var z = input;
        ////     var y = z;
        //// }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Nope)]
        private void PrivateMethod() => StaticMethod();

        // Method not referenced anywhere.
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = Nope)]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Nope)]
        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = Nope)]
        [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = Nope)]
        private void UnusedMethod()
        {
            var x = 1;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = Nope)]
        [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = Nope)]
        private static void StaticMethod()
        {
            var x = 1;
        }
    }
}
