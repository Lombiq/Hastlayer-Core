using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    // Sample for a type that implements an interface and is used through it, to test interface proxy generation. Will contain some
    // actual sample later.

    public interface IService
    {
        void Method();
    }


    public class ServiceSample : IService
    {
        public void Method()
        {
            var z = 5;
            var y = z;
        }
    }
}
