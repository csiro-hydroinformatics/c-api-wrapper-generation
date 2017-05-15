using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiWrapperGenerator;
using Xunit;

namespace TestApiWrapperGenerator
{
    public class MatlabWrapperGenerator
    {
        static string ClassName = "SwiftCApi";

        [Fact]
        public void Indentation()
        {
            string apiLine = @"SWIFT_API int ApiFun(MODEL_SIMULATION_PTR simulation);";

            HeaderFilter filter = new HeaderFilter();
            var gen = new MatlabApiWrapperGenerator();
            gen.UniformIndentationCount = 0;
            gen.Indentation = "    ";
            var filtered = filter.FilterInput(apiLine);
            var w = new WrapperGenerator(gen, filter);
            var result = w.Convert(filtered);
            Assert.Equal(1, result.Length);
            var s = result[0].Trim('\n');

            string[] expectedLines = {
"function f = ApiFun_m(simulation)",
//"% ApiFun_m",
//"%",
//"% INPUT simulation [libpointer] A SWIFT simulation object (i.e. a model runner)",
//"% OUTPUT some integer",
//"",
"simulation_xptr = getExternalPtr(simulation);",
"res = calllib('swift', 'ApiFun', simulation_xptr);",
"f = mkExternalObjRef(res);",
//"",
"end",
"", // newline
"" // newline, ie empty line after body.
            };

            CSharpWrapperGenerator.CheckWrappingFunction(filter, gen, apiLine, expectedLines);

        }

    }
}
