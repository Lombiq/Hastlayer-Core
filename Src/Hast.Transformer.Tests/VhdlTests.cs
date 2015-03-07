using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer;
using Hast.Transformer.Vhdl;
using NUnit.Framework;

namespace Hast.Transformer.Tests
{
    [TestFixture]
    public class VhdlTests
    {
        private readonly ITranspiler _transpiler;


        public VhdlTests()
        {
            _transpiler = new Transpiler(new TranspilingEngine(new TranspilingSettings { MaxDegreeOfParallelism = 10 }));
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

            var hardwareDef = _transpiler.Transpile(csharp, Language.CSharp);
        }
    }
}
