using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Moq;
using NUnit.Framework;
using Orchard.Tests.Utility;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class VhdlTransformerTests
    {
        private IContainer _container;

        private ITransformer _transformer;


        [SetUp]
        public virtual void Init()
        {
            var builder = new ContainerBuilder();


            builder.RegisterAutoMocking(MockBehavior.Loose);

            builder.RegisterType<VhdlTransformingEngine>().As<ITransformingEngine>();

            _container = builder.Build();

            _transformer = _container.Resolve<ITransformer>();
        }

        [TestFixtureTearDown]
        public void Clean()
        {
        }


        [Test]
        public void HelloWorldTransformsCorrectly()
        {
            var csharp = @"
                public class HelloClass
                {
                    public string HelloMethod(string name)
                    {
                        return ""Hello"" + name;
                    }
                }";

            var hardwareDescription = _transformer.Transform(csharp, Language.CSharp, HardwareGenerationConfiguration.Default).Result;
        }
    }
}
