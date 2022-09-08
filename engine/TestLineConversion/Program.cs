using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ApiWrapperGenerator;

namespace TestLineConversion
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.Error.WriteLine("Usage:");
                Console.Error.WriteLine("TestLineConversion \"SWIFTAPI void blah();\" wrappertype");
                return;
            }
            string gen = args[1];
            //TestStrings();
            if(gen == "cs")
                TestCsWrpGen(args);
            else if (gen == "m")
                TestMatlabWrpGen(args);
            else if (gen == "r")
                TestRWrpGen(args);
            else if (gen == "p")
                TestPyWrpGen(args);
            else
                Console.Error.WriteLine("Unknown option " + gen);
        }

        private static IntPtr TestStrings()
        {
            string[] test = { "a", "bb" };
            IntPtr charpp = Marshal.AllocHGlobal(test.Length * IntPtr.Size);

            for (int i = 0; i < test.Length; i++)
            {
                int offset = i * IntPtr.Size;
                IntPtr p = Marshal.StringToHGlobalAnsi(test[i]);
                Marshal.WriteIntPtr(charpp, offset, p);
            }

            string[] reread = new string[2];
            for (int i = 0; i < test.Length; i++)
            {
                int offset = i * IntPtr.Size;
                IntPtr p = Marshal.ReadIntPtr(charpp, offset);
                //IntPtr pointer = IntPtr.Add(p, Marshal.SizeOf(typeof(VECTOR_SEXPREC)));
                reread[i] = Marshal.PtrToStringAnsi(p);
            }

            for (int i = 0; i < test.Length; i++)
            {
                int offset = i * IntPtr.Size;
                IntPtr p = Marshal.ReadIntPtr(charpp, offset);
                Marshal.FreeHGlobal(p);
            }
            Marshal.FreeHGlobal(charpp);

            return charpp;
        }

        private static void TestCsWrpGen(string[] args)
        {
            var gen = new CsharpApiWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            ProcessTestLine(args, gen);
        }

        private static void ProcessTestLine(string[] args, IApiConverter gen, HeaderFilter filter = null)
        {
            if(filter == null)
                filter = createFilter();
            string apiLine = args[0];
            apiLine = filter.FilterInput(apiLine)[0];
            var w = new WrapperGenerator(gen, filter);
            var result = w.Convert(new string[] { apiLine });
            Console.WriteLine(result[0]);
        }

        private static void TestMatlabWrpGen(string[] args)
        {
            var gen = new MatlabApiWrapperGenerator();
            ProcessTestLine(args, gen);
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
        }
        
        private static void TestPyWrpGen(string[] args)
        {
            var gen = new PythonCffiWrapperGenerator();
            var cffiObjName = "libname_so"; 

            gen.FunctionNamePostfix = "_py";
            gen.ApiCallPrefix = cffiObjName + "." ;
            gen.ApiCallPostfix = "";

            gen.FunctionNamePostfix = "_py";
            gen.ApiCallPostfix = "";
            gen.GeneratePyDocstringDoc = true ;
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
            gen.SetTransientArgConversion( "char**"  , "_charpp", "C_ARGNAME = wrap_as_pointer_handle(marshal.as_arrayof_bytes(RCPP_ARGNAME))", "# clean C_ARGNAME ?");
            gen.SetTypeMap("char*", "str");
            gen.SetTransientArgConversion( "char*"  , "_charp", "C_ARGNAME = wrap_as_pointer_handle(as_bytes(RCPP_ARGNAME))", "# no cleanup for char*?");
            gen.SetTypeMap("const char*", "str");
            gen.SetTransientArgConversion( "const char*" , "_c_charp", "C_ARGNAME = wrap_as_pointer_handle(as_bytes(RCPP_ARGNAME))", "# no cleanup for const char*");
            gen.SetTransientArgConversion( "double**", "_doublepp", "C_ARGNAME = wrap_as_pointer_handle(as_double_ptr_array(RCPP_ARGNAME))", "# delete[] C_ARGNAME");
            gen.SetTransientArgConversion( ".*_PTR"  , "_xptr", "C_ARGNAME = wrap_as_pointer_handle(RCPP_ARGNAME)", "")  ;
            //;
            gen.ClearCustomWrappers();
            gen.FunctionWrappers = "@check_exceptions";
            //;
            var returnscharptrptr = gen.ReturnsCharPtrPtrWrapper();
            gen.AddCustomWrapper(returnscharptrptr);
            var returnsdoubleptr = gen.ReturnsDoublePtrWrapper();
            gen.AddCustomWrapper(returnsdoubleptr);

            gen.SetTypeMap("character_vector*", "List");
            gen.SetReturnedValueConversion("character_vector*", "character_vector_to_list(C_ARGNAME, dispose=True)");
            gen.SetTypeMap("char*", "str");
            gen.SetReturnedValueConversion("char*", "char_array_to_py(C_ARGNAME, dispose=True)");
            // gen.SetReturnedValueConversion("char**", "charpp_to_list(C_ARGNAME, size, dispose=True)");
            gen.SetTypeMap("date_time_to_second", "datetime");
            gen.SetTypeMap("MarshaledDateTime", "datetime");
            gen.SetTypeMap("QppMarshaledDateTime", "datetime");

            gen.SetTransientArgConversion("date_time_to_second", "_datetime", 
                "C_ARGNAME = marshal.datetime_to_dtts(RCPP_ARGNAME)", 
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTransientArgConversion("MarshaledDateTime", "_datetime", 
                "C_ARGNAME = marshal.datetime_to_dtts(RCPP_ARGNAME)", 
                "# C_ARGNAME - no cleanup needed?");
            gen.SetTransientArgConversion("QppMarshaledDateTime", "_datetime", 
                "C_ARGNAME = marshal.datetime_to_dtts(RCPP_ARGNAME)", 
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTransientArgConversion("double*", "_numarray", 
                // Note that we can use shallow=True only if we know for sure the API will copy the data for internal purposes e.g. Input Player. Otherwise, dangerous
                "C_ARGNAME = marshal.as_c_double_array(RCPP_ARGNAME, shallow=True)", 
                "# C_ARGNAME - no cleanup needed?");

            gen.SetReturnedValueConversion("date_time_to_second", "marshal.as_datetime(C_ARGNAME)");
            gen.SetReturnedValueConversion("MarshaledDateTime", "marshal.as_datetime(C_ARGNAME)");
            gen.SetReturnedValueConversion("QppMarshaledDateTime", "marshal.as_datetime(C_ARGNAME)");

            gen.SetTypeMap("regular_time_series_geometry*", "TimeSeriesGeometryNative");
            gen.SetTypeMap("TS_GEOMETRY_PTR", "TimeSeriesGeometryNative");

            // gen.SetTransientArgConversion("regular_time_series_geometry*", "_tsgeom", 
            //     "C_ARGNAME = marshal.as_native_tsgeom(RCPP_ARGNAME)", 
            //     "# delete C_ARGNAME")

            // gen.SetTransientArgConversion("TS_GEOMETRY_PTR", "_tsgeom", 
            //     "C_ARGNAME = marshal.as_native_tsgeom(RCPP_ARGNAME)",
            //     "# delete C_ARGNAME")


            gen.SetTypeMap("multi_statistic_definition*", "Any");
            gen.SetTransientArgConversion("multi_statistic_definition*", "_mstatdef", 
                "C_ARGNAME = to_multi_statistic_definition(RCPP_ARGNAME)",
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTypeMap("named_values_vector*", "Dict[str,float]");
            gen.SetTransientArgConversion("named_values_vector*", "_nvv", 
                "C_ARGNAME = marshal.dict_to_named_values(RCPP_ARGNAME)",
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTypeMap("values_vector*", "List[float]");
            gen.SetTransientArgConversion("values_vector*", "_vv", 
                "C_ARGNAME = marshal.create_values_struct(RCPP_ARGNAME)",
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTypeMap("string_string_map*", "Dict[str,str]");
            gen.SetTransientArgConversion("string_string_map*", "_dict", 
                "C_ARGNAME = marshal.dict_to_string_map(RCPP_ARGNAME)",
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTypeMap("const multi_regular_time_series_data*", "xr.DataArray");
            gen.SetTransientArgConversion("const multi_regular_time_series_data*", "_tsd_ptr", 
                "C_ARGNAME = marshal.as_native_time_series(RCPP_ARGNAME)",
                "# C_ARGNAME - no cleanup needed?");

            gen.SetTypeMap("multi_regular_time_series_data*", "xr.DataArray");
            gen.SetReturnedValueConversion("multi_regular_time_series_data*", 
                "opaque_ts_as_xarray_time_series(C_ARGNAME, dispose=True)");

            gen.SetTypeMap("time_series_dimensions_description*", "List");
            gen.SetReturnedValueConversion("time_series_dimensions_description*", 
                "py_time_series_dimensions_description(C_ARGNAME, dispose=True)");

            // # Note that "&" is a not valid C concept... so the following should not be used. Legacy to be replaced with ptrs
            gen.SetTypeMap("const multi_regular_time_series_data&", "Any");
            gen.SetTransientArgConversion("const multi_regular_time_series_data&", "_tsd", 
                "C_ARGNAME = cinterop.timeseries.to_multi_regular_time_series_data(RCPP_ARGNAME)",
                "cinterop.disposal.dispose_of_multi_regular_time_series_data(C_ARGNAME)");

            gen.SetTypeMap("const regular_time_series_geometry&", "Any");
            gen.SetTransientArgConversion("const regular_time_series_geometry&", "_tsd", 
                "C_ARGNAME = cinterop.timeseries.to_regular_time_series_geometry(RCPP_ARGNAME)", 
                "# C_ARGNAME - no cleanup needed")  ;

            gen.SetTypeMap("OptimizerLogData*", "DeletableCffiNativeHandle");
            gen.SetReturnedValueConversion("OptimizerLogData*", 
                "custom_wrap_cffi_native_handle(C_ARGNAME, 'OptimizerLogData*', DisposeOptimizerLogDataWila_py)");

            gen.SetTypeMap("CatchmentStructure*", "DeletableCffiNativeHandle");
            gen.SetReturnedValueConversion("CatchmentStructure*", 
                "custom_wrap_cffi_native_handle(C_ARGNAME, 'CatchmentStructure*', DisposeCatchmentStructure_py)");

            gen.SetTypeMap("named_values_vector*", "Dict[str,float]");
            gen.SetReturnedValueConversion("named_values_vector*", 
                "named_values_to_py(C_ARGNAME, dispose=True)");

            gen.SetTypeMap("SceParameters", "Dict");
            gen.SetTransientArgConversion("SceParameters", "_sceparams", "C_ARGNAME = toSceParametersNative(RCPP_ARGNAME)", "# C_ARGNAME - no cleanup needed");

            ProcessTestLine(args, gen);
        }

        private static void TestRWrpGen(string[] args)
        {
            var gen = new RXptrWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            ProcessTestLine(args, gen);
        }

        private static HeaderFilter createFilter()
        {
            var apiFilter = new HeaderFilter();
            apiFilter.ContainsAny = new string[] { "SWIFT_API" };
            apiFilter.ToRemove = new string[] { "SWIFT_API" };
            return apiFilter;
        }
    }
}
