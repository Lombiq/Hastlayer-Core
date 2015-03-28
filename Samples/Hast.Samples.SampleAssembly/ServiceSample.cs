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


    public class ServiceSample : ServiceBase, IService
    {
        // Explicit interface implementation.
        int IService.Method1()
        {
            return PrivateMethod() + 5;
        }

        // Implicit interface implementation.
        public int Method2()
        {
            if (Base())
            {
                return 4;
            }
            return PrivateMethod() + PrivateMethod() - 3;
        }

        public virtual bool Unused()
        {
            return true;
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


    public class ServiceBase
    {
        protected bool Base()
        {
            return true;
        }
    }
}
