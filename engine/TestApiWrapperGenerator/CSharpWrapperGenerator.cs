using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ApiWrapperGenerator;
using Xunit;
using Xunit.Sdk;

namespace TestApiWrapperGenerator
{
    public class CSharpWrapperGenerator
    {
        static string ClassName = "SwiftCApi";


        [Fact]
        public void TestRemoveBlankLines()
        {
            string s = "    \n    int a = 1;\n    \n    double b = 0;\n    \n\n\n";
            Assert.Equal(StringHelper.RemoveBlankLines(s), "    int a = 1;\n    double b = 0;\n");
        }

        [Fact]
        public void Indentation()
        {
            string apiLine = @"SWIFT_API int ApiFun(MODEL_SIMULATION_PTR simulation);";

            HeaderFilter filter = new HeaderFilter();
            var genDelegate = new CsharpDelegatesApiWrapperGenerator();
            genDelegate.UniformIndentationCount = 2;
            genDelegate.Indentation = "    ";
            var filtered = filter.FilterInput(apiLine);
            var w = new WrapperGenerator(genDelegate, filter);
            var result = w.Convert(filtered);
            Assert.Single(result);
            var s = result[0].Trim('\n');
            string expected = genDelegate.Indentation + genDelegate.Indentation + @"private delegate int ApiFun_csdelegate(IntPtr simulation);";
            Assert.Equal(expected, s);

            string[] expectedLines = {
"        int ApiFun_cs(IModelSimulation simulation)",
"        {",
"            var result = "+ClassName+".NativeSwiftLib.GetFunction<ApiFun_csdelegate>(\"ApiFun\")(CheckedDangerousGetHandle(simulation, \"simulation\"));",
"            var x = result;",
"            return x;",
"        }",
"", // newline
"" // newline, ie empty line after body.
            };

            var gen = new CsharpApiWrapperGenerator();
            gen.UniformIndentationCount = 2;
            gen.Indentation = "    ";

            CheckWrappingFunction(filter, gen, apiLine, expectedLines);

        }


        [Fact]
        public void ReturnsCharPtrPtrToDelegate()
        {

            string apiLine = @"SWIFT_API char** ApiFun(MODEL_SIMULATION_PTR simulation, char** elementIds, int numElements, int* size);";

            HeaderFilter filter = new HeaderFilter();
            var gen = new CsharpDelegatesApiWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            var filtered = filter.FilterInput(apiLine);
            var w = new WrapperGenerator(gen, filter);
            var result = w.Convert(filtered);
            string[] lines = SplitToLines(result);
            Assert.Equal(2, lines.Length);
            var s = lines[0].Trim();
            string expected = @"private delegate IntPtr ApiFun_csdelegate(IntPtr simulation, IntPtr elementIds, int numElements, IntPtr size);";
            Assert.Equal(expected, s);
        }

        [Fact]
        public void CustomCharPPFunctionGenerator()
        {
            HeaderFilter filter = new HeaderFilter();
            var gen = new CsharpApiWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());

            string apiLine = @"SWIFT_API char** ApiFun(MODEL_SIMULATION_PTR simulation, char** elementIds, char** elementIds2, int numElements, int* size);";

            string[] expectedLines = new string[] {
"string[] ApiFun_cs(IModelSimulation simulation, string[] elementIds, string[] elementIds2, int numElements)",
"{",
"    IntPtr size = InteropHelper.AllocHGlobal<int>();",
"    IntPtr elementIds_charpp = InteropHelper.ArrayStringToHGlobalAnsi(elementIds);",
"    IntPtr elementIds2_charpp = InteropHelper.ArrayStringToHGlobalAnsi(elementIds2);",
"    IntPtr result = " + ClassName + ".NativeSwiftLib.GetFunction<ApiFun_csdelegate>(\"ApiFun\")(CheckedDangerousGetHandle(simulation, \"simulation\"), elementIds_charpp, elementIds2_charpp, numElements, size);",
"    InteropHelper.FreeHGlobalAnsiString(elementIds_charpp, elementIds.Length);",
"    InteropHelper.FreeHGlobalAnsiString(elementIds2_charpp, elementIds2.Length);",
"    int n = InteropHelper.Read<int>(size, true);",
"    return InteropHelper.GlobalAnsiToArrayString(result, n, true);",
"}",
"", // newline
"" // newline, ie empty line after body.
            };

            CheckWrappingFunction(filter, gen, apiLine, expectedLines);

        }

        [Fact]
        public void NetworkInfo()
        {
            HeaderFilter filter = new HeaderFilter();
            var gen = new CsharpApiWrapperGenerator();

            //string apiLine = "SWIFT_API MODEL_SIMULATION_PTR CreateNewFromNetworkInfo(NODE_INFO_PTR nodes, int numNodes, LINK_INFO_PTR links, int numLinks);";
            
        }

        [Fact]
        public void TimeSeriesGeometry()
        {
            HeaderFilter filter = new HeaderFilter();
            var gen = new CsharpApiWrapperGenerator();

            string apiLine = "SWIFT_API void Play(MODEL_SIMULATION_PTR simulation, const char* variableIdentifier, double * values, TS_GEOMETRY_PTR geom);";

            string[] expectedLines = new string[] {
"void Play_cs(IModelSimulation simulation, string variableIdentifier, double[] values, ref MarshaledTimeSeriesGeometry geom)",
"{",
"    IntPtr values_doublep = InteropHelper.ArrayDoubleToNative(values);",
"    IntPtr geom_struct = InteropHelper.StructureToPtr(geom);",
"    " + ClassName + ".NativeSwiftLib.GetFunction<Play_csdelegate>(\"Play\")(CheckedDangerousGetHandle(simulation, \"simulation\"), variableIdentifier, values_doublep, geom_struct);",
"    InteropHelper.CopyDoubleArray(values_doublep, values, true);",
"    InteropHelper.FreeNativeStruct(geom_struct, ref geom, true);",
"}",
"", // newline
"" // newline, ie empty line after body.
    };
            CheckWrappingFunction(filter, gen, apiLine, expectedLines);
        }

        [Fact]
        public void DateTimeMarshal()
        {
            HeaderFilter filter = new HeaderFilter();
            var gen = new CsharpApiWrapperGenerator();

            string apiLine = "SWIFT_API void GetStart(SIMULATION_BASE_PTR simulation, DATE_TIME_INFO_PTR start);";

            string[] expectedLines = new string[] {
"void GetStart_cs(IModelSimulation simulation, ref MarshaledDateTime start)",
"{",
"    IntPtr start_struct = InteropHelper.StructureToPtr(start);",
"    " + ClassName + ".NativeSwiftLib.GetFunction<GetStart_csdelegate>(\"GetStart\")(CheckedDangerousGetHandle(simulation, \"simulation\"), start_struct);",
"    InteropHelper.FreeNativeStruct(start_struct, ref start, true);",
"}",
"", // newline
"" // newline, ie empty line after body.
            };
            CheckWrappingFunction(filter, gen, apiLine, expectedLines);
        }

        [Fact]
        public void AggregateParameterizersWrapper()
        {
            HeaderFilter filter = new HeaderFilter();
            var gen = new CsharpApiWrapperGenerator();

            string apiLine = "SWIFT_API COMPOSITE_PARAMETERIZER_PTR AggregateParameterizers(const char* strategy, ARRAY_OF_PARAMETERIZERS_PTR parameterizers, int numParameterizers);";

            string[] expectedLines = new string[] {
"INativeParameterizer AggregateParameterizers_cs(string strategy, INativeParameterizer[] parameterizers, int numParameterizers)",
"{",
"    IntPtr parameterizers_array_ptr = InteropHelper.CreateNativeArray(Array.ConvertAll(parameterizers, p => p.GetHandle()));",
"    var result = " + ClassName + ".NativeSwiftLib.GetFunction<AggregateParameterizers_csdelegate>(\"AggregateParameterizers\")(strategy, parameterizers_array_ptr, numParameterizers);",
"    InteropHelper.DeleteNativeArray(parameterizers_array_ptr);",
"    var x = createWrapperNativeParameterizer(result);",
"    return x;",
"}",
"", // newline
"" // newline, ie empty line after body.
            };
            CheckWrappingFunction(filter, gen, apiLine, expectedLines);
        }

        public static void CheckWrappingFunction(HeaderFilter filter, IApiConverter gen, string apiLine, string[] expectedLines, bool strict=true)
        {
            var filtered = filter.FilterInput(apiLine);
            var w = new WrapperGenerator(gen, filter);
            var result = w.Convert(filtered);
            string[] lines = SplitToLines(result);
            // To debug on the cheap...:
            // foreach (string x in lines)
            // {
            //     Console.WriteLine(x);
            // }
            // if (!strict)
            //     lines = lines.Select(x => x.Trim()).ToArray();
            CheckExpectedLines(expectedLines, lines);
        }

        public static void CheckExpectedLines(string[] expectedLines, string[] lines)
        {
            bool samelen = (expectedLines.Length == lines.Length);
            bool same_lines = true;
            for (int i = 0; i < lines.Length; i++)
                if (expectedLines[i] != lines[i])
                {
                    var s = String.Format("expected line:\n {0} \n but got:\n {1} \n", expectedLines[i], lines[i]);
                    Console.WriteLine(s);
                    same_lines= false;
                }
            if (!samelen || !same_lines)
            {
                var expected = string.Join("\n", expectedLines);
                var actual = string.Join("\n", lines);
                var s = String.Format("expected:\n {0} \n but got:\n {1} \n", expected, actual);
                Console.WriteLine(s);
                Assert.Fail("Generated and expected code lines differ");
            }
        }

        public static string[] SplitToLines(string s)
        {
            return SplitToLines(new string[] { s });
        }

        public static string[] SplitToLines(string[] result)
        {
            // for (int i = 0; i < result.Length; i++)
            // {
            //     if (result[i].Contains("\r"))
            //         throw new FormatException("A line contains a carriage return character");
            // }
            Assert.Single(result);
            string[] lines = result[0].Split(new string[] { /*Environment.NewLine, */"\n" }, StringSplitOptions.None);
            //lines = (from l in lines select l.Trim()).ToArray();
            //lines = (from l in lines select l.Replace("  ", " ")).ToArray();
            return lines;
        }
    }
}
