using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    // Sample for a type that implements an interface and is used through it, to test interface proxy generation. Will contain some
    // actual sample later.
    public interface IService : IServiceBase
    {
        int Method1();
    }


    public interface IServiceBase
    {
        int Method2();
    }


    public class ServiceSample : IService
    {
        // Explicit interface implementation.
        int IService.Method1()
        {
            return PrivateMethod() + 5;
        }

        // Implicit interface implementation.
        public int Method2()
        {
            return PrivateMethod() + PrivateMethod() - 3;
        }

        
        private int PrivateMethod()
        {
            return 5 + StaticMethod();
        }


        private static int StaticMethod()
        {
            return 7;
        }
    }
}
