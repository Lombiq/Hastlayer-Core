using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HastTranspiler;
using HastTranspiler.Vhdl;
using NUnit.Framework;

namespace HastTranspilerTests
{
    [TestFixture]
    public class VhdlTests
    {
        private readonly ITranspiler _transpiler;


        public VhdlTests()
        {
            _transpiler = new Transpiler(new TranspilingEngine());
        }


        [Test]
        public void Test()
        {
            var csharp = @"
                public class HelloClass
                {
                    public string HelloMethod(string name)
                    {
                        return ""Hello"" + name;
                    }
                }";

            var vhdl = _transpiler.Transpile(csharp, Language.CSharp);
            Assert.IsNotNullOrEmpty(vhdl);
        }
    }
}
