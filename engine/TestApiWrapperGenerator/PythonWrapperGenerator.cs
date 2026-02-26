using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using ApiWrapperGenerator;
using Xunit;

namespace TestApiWrapperGenerator
{
    public class PythonWrapperGenerator
    {
        static string ClassName = "SwiftCApi";

        private PythonCffiWrapperGenerator createGenerator()
        {
            var gen = new PythonCffiWrapperGenerator();
            var cffiObjName = "libname_so"; 

            gen.FunctionNamePostfix = "_py";
            gen.ApiCallPrefix = cffiObjName + "." ;
            gen.ApiCallPostfix = "";
            gen.GeneratePyDocstringDoc = false ;
            gen.UniformIndentationCount = 0;
            string prependHeader = @"#####
            # PREAMBLE
            #####
            ";
            // makePrependCodegen pkgName cffiObjName wrapperClassNames otherImports nativeDisposeFunction

            gen.ApiCallPrefix = cffiObjName + ".";
            gen.PrependOutputFile = prependHeader;
            // gen.RoxygenDocPostamble = "#' @export") ;
            gen.CreateXptrObjRefFunction = "custom_wrap_cffi_native_handle";
            gen.GetXptrFromObjRefFunction = "unwrap_cffi_native_handle";

            gen.ClearCustomWrappers();
            gen.FunctionWrappers = "@check_exceptions";
            //;
            var returnscharptrptr = gen.ReturnsCharPtrPtrWrapper();
            gen.AddCustomWrapper(returnscharptrptr);
            var returnsdoubleptr = gen.ReturnsDoublePtrWrapper();
            gen.AddCustomWrapper(returnsdoubleptr);
            gen.SetTransientArgConversion( ".*_PTR"  , "_xptr", "C_ARGNAME = wrap_as_pointer_handle(RCPP_ARGNAME)", "")  ;

            return gen;
        }

        [Fact]
        public void Indentation()
        {
            string apiLine = @"SWIFT_API int ApiFun(MODEL_SIMULATION_PTR simulation);";

            HeaderFilter filter = new HeaderFilter();
            var pyGen = createGenerator();
            pyGen.GeneratePyDocstringDoc = true;
            pyGen.Indentation = "    ";
            pyGen.FunctionNamePostfix = "_py_post";

            string[] expectedLines = {
"@check_exceptions",
"def _ApiFun_native(simulation):",
"    result = libname_so.ApiFun(simulation)",
"    return result",
"",
"def ApiFun_py_post(simulation:Any) -> int:",
"    \"\"\"ApiFun_py_post",
"    ",
"    ApiFun_py_post: generated wrapper function for API function ApiFun",
"    ",
"    Args:",
"        simulation (Any): simulation",
"    ",
"    Returns:",
"        (int): returned result",
"    ",
"    \"\"\"",
"    simulation_xptr = wrap_as_pointer_handle(simulation)",
"    result = _ApiFun_native(simulation_xptr.ptr)",
"    return result",
"", 
"", 
""};

            CheckWrappingFunction(filter, pyGen, apiLine, expectedLines);

        }

        public static void CheckWrappingFunction(HeaderFilter filter, IApiConverter gen, string apiLine, string[] expectedLines, bool strict=true)
        {
            CSharpWrapperGenerator.CheckWrappingFunction(filter, gen, apiLine, expectedLines, strict);
        }

        public static void CheckExpectedLines(string[] expectedLines, string[] lines)
        {
            CSharpWrapperGenerator.CheckExpectedLines(expectedLines, lines);
        }

        public static string[] SplitToLines(string s)
        {
            return SplitToLines(new string[] { s });
        }

        public static string[] SplitToLines(string[] result)
        {
            return CSharpWrapperGenerator.SplitToLines(result);
        }
    }
}
