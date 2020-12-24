using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiWrapperGenerator;
using Xunit;

namespace TestApiWrapperGenerator
{
    public class RcppWrapperGenerator
    {

        [Fact]
        public void FunctionReturnsValueFormattingCppGlue()
        {
            string apiLine = @"SWIFT_API int ApiFun();";

            HeaderFilter filter = new HeaderFilter();
            var gen = new RcppGlueWrapperGenerator();
            gen.UniformIndentationCount = 2;
            gen.Indentation = "    ";
            gen.FunctionNamePostfix = "_RcppPostfix";
            string[] expectedLines = {
"        // [[Rcpp::export]]",
"        IntegerVector ApiFun_RcppPostfix()",
"        {",
"            auto result = ApiFun();",
"            auto x = Rcpp::wrap(result);",
"            return x;",
"        }",
"", // newline
"" // newline, ie empty line after body.
            };

            CSharpWrapperGenerator.CheckWrappingFunction(filter, gen, apiLine, expectedLines);

        }


        [Fact]
        public void FunctionReturnsValueFormattingRGlue()
        {
            string apiLine = @"	SWIFT_API int GetNumRunoffModelVarIdentifiers(const char* modelId);";

            HeaderFilter filter = new HeaderFilter();
            var gen = new RXptrWrapperGenerator();
            gen.UniformIndentationCount = 0;
            gen.Indentation = "  ";
            gen.FunctionNamePostfix = "_RPostfix";

            gen.RoxyExportFunctions = true;


            gen.GenerateRoxygenDoc = true;
            StringBuilder sb = new StringBuilder();
            FuncAndArgs faa = new FuncAndArgs(apiLine);
            Assert.True(gen.CreateWrapFuncRoxydoc(sb, faa));

            var generatedLines= CSharpWrapperGenerator.SplitToLines(sb.ToString());

            string[] expectedLines = {
"#' GetNumRunoffModelVarIdentifiers_RPostfix",
"#'",
"#' GetNumRunoffModelVarIdentifiers_RPostfix Wrapper function for GetNumRunoffModelVarIdentifiers",
"#'",
"#' @param modelId R type equivalent for C++ type const char*",
"#' @export",
"" // newline
};

            CSharpWrapperGenerator.CheckExpectedLines(expectedLines, generatedLines);

            gen.GenerateRoxygenDoc = false;

            gen.ApiCallPostfix = "_Rcpp";
            expectedLines = new string[] {
"GetNumRunoffModelVarIdentifiers_RPostfix <- function(modelId) {",
"  modelId <- getSwiftXptr(modelId)",
"  result <- GetNumRunoffModelVarIdentifiers_Rcpp(modelId)",
"  return(mkSwiftObjRef(result, 'int'))",
"}",
"", // newline
"" // newline, ie empty line after body.
            };

            CSharpWrapperGenerator.CheckWrappingFunction(filter, gen, apiLine, expectedLines);

        }

    }
}
