using System;
using System.Collections.Generic;
using System.Text;

namespace ApiWrapperGenerator
{
    public abstract class BaseCsharpApiWrapperGenerator : BaseApiConverter
    {
        public string NativeHandleInterfaceName;

        protected BaseCsharpApiWrapperGenerator()
        {
            DelegateFunctionNamePostfix = "_csdelegate";
            ReturnedValueDeclarationKeyword = "var";
            NativeHandleInterfaceName = "ISwiftNativeHandle";
            AssignmentSymbol = "=";
            ReturnedValueVarname = "result";
            FunctionNamePostfix = "_cs";
            //OpaquePointers = false;
            DeclarationOnly = false;
            //AddCsharpExport = true;
            NewLineString = StringHelper.NewLineString;

            SetTypeMap("void", "void");
            SetTypeMap("int", "int");
            SetTypeMap("char*", "string");
            SetTypeMap("char", "string");
            SetTypeMap("double", "double");
            SetTypeMap("double*", "double[]");
            SetTypeMap("double**", "double[][]");
            SetTypeMap("bool", "bool");
            SetTypeMap("const char", "string");
            SetTypeMap("const int", "int");
            SetTypeMap("const double", "double");
            SetTypeMap("const char*", "string");
            SetTypeMap("const int*", "int[]");
            SetTypeMap("const double*", "double[]");

            SetTypeMap("MODEL_SIMULATION_PTR", "IModelSimulation");
            SetTypeMap("SIMULATION_BASE_PTR", "IModelSimulation");
            SetTypeMap("MEMORY_STATES_PTR", NativeHandleInterfaceName);
            SetTypeMap("OBJECTIVE_EVALUATOR_PTR", "ObjectiveCalculator");
            SetTypeMap("HYPERCUBE_PTR", "INativeParameterizer");
            SetTypeMap("PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("COMPOSITE_PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("CONSTRAINT_PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("SCALING_PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("STATE_INIT_PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("TRANSFORM_PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("SUBAREAS_SCALING_PARAMETERIZER_PTR", "INativeParameterizer");
            SetTypeMap("STATE_INITIALIZER_PTR", "IStateInitializer");
            SetTypeMap("TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetTypeMap("ENSEMBLE_DATA_SET_PTR", NativeHandleInterfaceName);
            SetTypeMap("ENSEMBLE_FORECAST_TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetTypeMap("ENSEMBLE_TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetTypeMap("ENSEMBLE_PTR_TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetTypeMap("TIME_SERIES_PROVIDER_PTR", NativeHandleInterfaceName);
            SetTypeMap("ENSEMBLE_FORECAST_SIMULATION_PTR", NativeHandleInterfaceName);
            SetTypeMap("ENSEMBLE_SIMULATION_PTR", NativeHandleInterfaceName);
            SetTypeMap("CANDIDATE_FACTORY_SEED_WILA_PTR", NativeHandleInterfaceName);
            SetTypeMap("OBJECTIVE_EVALUATOR_WILA_PTR", "ObjectiveCalculator");
            SetTypeMap("OBJECTIVE_SCORES_WILA_PTR", NativeHandleInterfaceName);
            SetTypeMap("HYPERCUBE_WILA_PTR", NativeHandleInterfaceName);
            SetTypeMap("SCE_TERMINATION_CONDITION_WILA_PTR", NativeHandleInterfaceName);
            SetTypeMap("OPTIMIZER_PTR", NativeHandleInterfaceName);
            SetTypeMap("VEC_OBJECTIVE_SCORES_PTR", NativeHandleInterfaceName);
            SetTypeMap("VOID_PTR_PROVIDER_PTR", NativeHandleInterfaceName);


            PrependOutputFile = string.Format("// This file was GENERATED{0}//Do NOT modify it manually, as you are very likely to lose work{0}{0}", EnvNewLine);

            ClassName = "SwiftCApi";

        }

        public string ClassName { get; set; }

        public string DelegateFunctionNamePostfix { get; set; }

        protected string AnsiCToCsharpTypes(string rt)
        {
            return DefaultAnsiCToWrapperType(rt);
        }

        protected void ApiArgToCsharp(StringBuilder sb, TypeAndName typeAndName)
        {
            var rt = typeAndName.TypeName;
            ApiTypeToCsharpApi(sb, rt);
            sb.Append(" ");
            sb.Append(typeAndName.VarName);
        }

        protected abstract void ApiTypeToCsharpApi(StringBuilder sb, string typename);


        protected void ReturnApiTypeToCsharpDelegateArgType(StringBuilder sb, string typename)
        {
            if (IsPointer(typename))
                sb.Append("IntPtr");
            else if (IsKnownType(typename))
                sb.Append(AnsiCToCsharpTypes(typename));
            else
                sb.Append(AnsiCToCsharpTypes(typename));
        }

        protected void ApiTypeToCsharpDelegateArgType(StringBuilder sb, string typename)
        {
            if (IsCharPtr(typename))
                sb.Append(AnsiCToCsharpTypes(typename));
            else if (IsPointer(typename))
                sb.Append("IntPtr");
            else if (IsKnownType(typename))
                sb.Append(AnsiCToCsharpTypes(typename));
            else
                sb.Append(AnsiCToCsharpTypes(typename));
        }

        protected void ApiArgToCsharpDelegateArg(StringBuilder sb, TypeAndName typeAndName)
        {
            var rt = typeAndName.TypeName;
            ApiTypeToCsharpDelegateArgType(sb, rt);
            sb.Append(" ");
            sb.Append(typeAndName.VarName);
        }

        protected void ReturnApiArgToCsharpDelegateArg(StringBuilder sb, TypeAndName typeAndName)
        {
            var rt = typeAndName.TypeName;
            ReturnApiTypeToCsharpDelegateArgType(sb, rt);
            sb.Append(" ");
            sb.Append(typeAndName.VarName);
        }


    }
    public class CsharpApiWrapperGenerator : BaseCsharpApiWrapperGenerator
    {
        public CsharpApiWrapperGenerator()
        {

            SetInstanceTypeMap("MODEL_SIMULATION_PTR", "ModelSimulation");
            SetInstanceTypeMap("SIMULATION_BASE_PTR", "ModelSimulation");
            SetInstanceTypeMap("MEMORY_STATES_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("OBJECTIVE_EVALUATOR_PTR", "ObjectiveCalculator");
            SetInstanceTypeMap("HYPERCUBE_PTR", "NativeParameterizer");
            SetInstanceTypeMap("PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("COMPOSITE_PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("CONSTRAINT_PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("SCALING_PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("STATE_INIT_PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("TRANSFORM_PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("SUBAREAS_SCALING_PARAMETERIZER_PTR", "NativeParameterizer");
            SetInstanceTypeMap("STATE_INITIALIZER_PTR", "StateInitializer");
            SetInstanceTypeMap("TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("ENSEMBLE_DATA_SET_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("ENSEMBLE_FORECAST_TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("ENSEMBLE_TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("ENSEMBLE_PTR_TIME_SERIES_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("TIME_SERIES_PROVIDER_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("ENSEMBLE_FORECAST_SIMULATION_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("ENSEMBLE_SIMULATION_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("CANDIDATE_FACTORY_SEED_WILA_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("OBJECTIVE_EVALUATOR_WILA_PTR", "ObjectiveCalculator");
            SetInstanceTypeMap("OBJECTIVE_SCORES_WILA_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("HYPERCUBE_WILA_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("SCE_TERMINATION_CONDITION_WILA_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("OPTIMIZER_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("VEC_OBJECTIVE_SCORES_PTR", NativeHandleInterfaceName);
            SetInstanceTypeMap("VOID_PTR_PROVIDER_PTR", NativeHandleInterfaceName);

            SetTypeMap("char**", "string[]");
            SetTransientArgConversion(
                "char**",
                "_charpp",
                // "IntPtr elementIds_charpp = InteropHelper.ArrayStringToHGlobalAnsi(elementIds);"
                // "IntPtr C_ARGNAME = InteropHelper.ArrayStringToHGlobalAnsi(RCPP_ARGNAME);"
                "IntPtr C_ARGNAME = InteropHelper.ArrayStringToHGlobalAnsi(RCPP_ARGNAME);",
                // "InteropHelper.FreeHGlobalAnsiString(elementIds_charpp, elementIds.Length);",
                // "InteropHelper.FreeHGlobalAnsiString(C_ARGNAME, RCPP_ARGNAME.Length);",
                "InteropHelper.FreeHGlobalAnsiString(C_ARGNAME, RCPP_ARGNAME.Length);");

            // All structs are marshalled in a similar manner:
            string createPtr =
                // IntPtr geom_struct = InteropHelper.StructureToPtr(geom);
                // IntPtr C_ARGNAME = InteropHelper.StructureToPtr(RCPP_ARGNAME);
                "IntPtr C_ARGNAME = InteropHelper.StructureToPtr(RCPP_ARGNAME);";
            string freePtr =
                // InteropHelper.FreeNativeStruct(geom_struct, ref geom, true);
                // InteropHelper.FreeNativeStruct(C_ARGNAME, ref RCPP_ARGNAME, true);
                "InteropHelper.FreeNativeStruct(C_ARGNAME, ref RCPP_ARGNAME, true);";

            SetTypeMap("TS_GEOMETRY_PTR", "ref MarshaledTimeSeriesGeometry");
            SetTransientArgConversion(
                "TS_GEOMETRY_PTR",
                "_struct", createPtr, freePtr);

            SetTypeMap("DATE_TIME_INFO_PTR", "ref MarshaledDateTime");
            SetTransientArgConversion(
                "DATE_TIME_INFO_PTR",
                "_struct", createPtr, freePtr);

            string createArrayStructPtr =
                "IntPtr C_ARGNAME = InteropHelper.ArrayOfStructureToPtr(RCPP_ARGNAME);";
            string freeArrayStructPtr =
                "InteropHelper.FreeNativeArrayOfStruct(C_ARGNAME, ref RCPP_ARGNAME, false);";

//#define NODE_INFO_PTR  NodeInfoTxt*
            SetTypeMap("NODE_INFO_PTR", "NodeInfo[]");
            SetTransientArgConversion(
                "NODE_INFO_PTR",
                "_struct", createArrayStructPtr, freeArrayStructPtr);

//#define LINK_INFO_PTR  LinkInfoTxt*
            SetTypeMap("LINK_INFO_PTR", "LinkInfo[]");
            SetTransientArgConversion(
                "LINK_INFO_PTR",
                "_struct", createArrayStructPtr, freeArrayStructPtr);


            SetTransientArgConversion("double**", "_doublepp", 
                "IntPtr C_ARGNAME = InteropHelper.BiArrayDoubleToNative(RCPP_ARGNAME);", 
                "InteropHelper.FreeBiArrayDouble(C_ARGNAME, RCPP_ARGNAME.Length);");
            SetTransientArgConversion("double*", "_doublep", 
                "IntPtr C_ARGNAME = InteropHelper.ArrayDoubleToNative(RCPP_ARGNAME);",
                "InteropHelper.CopyDoubleArray(C_ARGNAME, RCPP_ARGNAME, true);");

            //SWIFT_API COMPOSITE_PARAMETERIZER_PTR AggregateParameterizers(const char* strategy, ARRAY_OF_PARAMETERIZERS_PTR parameterizers, int numParameterizers);
            //         INativeParameterizer AggregateParameterizers_cs(string strategy, INativeParameterizer[] parameterizers, int numParameterizers)
            SetTypeMap("ARRAY_OF_PARAMETERIZERS_PTR", "INativeParameterizer[]");
            SetTransientArgConversion(
                "ARRAY_OF_PARAMETERIZERS_PTR",
                "_array_ptr",
                //IntPtr parameterizers_array_ptr = InteropHelper.CreateNativeArray(Array.ConvertAll(parameterizers, p => p.GetHandle()));
                "IntPtr C_ARGNAME = InteropHelper.CreateNativeArray(Array.ConvertAll(RCPP_ARGNAME, p => p.GetHandle()));",
                "InteropHelper.DeleteNativeArray(C_ARGNAME);");

        }

        public void SetInstanceTypeMap(string cType, string rcppType)
        {
            instanceTypeMap[cType] = rcppType;
        }
        private Dictionary<string, string> instanceTypeMap = new Dictionary<string, string>();


        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            // SWIFT_API CONSTRAINT_PARAMETERIZER_PTR CreateMuskingumConstraint(HYPERCUBE_PTR hypercubeParameterizer, double deltaT, const char* paramNameK, const char* paramNameX, MODEL_SIMULATION_PTR simulation);

            //private delegate IntPtr CreateMuskingumConstraint_csdelegate(IntPtr parameterizer, double deltaT, string paramNameK, string paramNameX, IntPtr simulation);
            //public INativeParameterizer CreateMuskingumConstraint(INativeParameterizer parameterizer, double deltaT, string paramNameK, string paramNameX, IModelSimulation simulation)
            //{
            //    if (parameterizer == null) throw new ArgumentNullException("parameterizer");
            //    IntPtr p = MsDotnetNativeApi.NativeSwiftLib.GetFunction<CreateMuskingumConstraint_csdelegate>("CreateMuskingumConstraint")
            //            (parameterizer.GetHandle(), deltaT, paramNameK, paramNameX, (simulation == null ? IntPtr.Zero : simulation.GetHandle()));
            //    return createParamaterizerWrapper(p);
            //}

            var sb = new StringBuilder();

            if (!createWrapFuncSignature(sb, funcAndArgs)) return line;
            if (DeclarationOnly)
            {
                sb.Append(StatementSep);
                return sb.ToString();
            }
            else
            {
                string result = "";
                result = createWrappingFunctionBody(line, funcAndArgs, sb, ApiCallArgument);
                return result;
            }
        }

        private void ApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            CsharpApiToCApiType(sb, typeAndName.TypeName, typeAndName.VarName);
        }

        protected override void CreateApiFunctionCallFunction(StringBuilder sb, TypeAndName funcDef)
        {
            // SWIFT_API CONSTRAINT_PARAMETERIZER_PTR CreateMuskingumConstraint(HYPERCUBE_PTR hypercubeParameterizer, double deltaT, const char* paramNameK, const char* paramNameX, MODEL_SIMULATION_PTR simulation);
            // should lead to a function call such as:
            //    MsDotnetNativeApi.NativeSwiftLib.GetFunction<CreateMuskingumConstraint_csdelegate>("CreateMuskingumConstraint")
            //            (parameterizer.GetHandle(), deltaT, paramNameK, paramNameX, (simulation == null ? IntPtr.Zero : simulation.GetHandle()));

            string getFunction = string.Format(ClassName + ".NativeSwiftLib.GetFunction<{0}{1}>(\"{0}\")",
                funcDef.VarName, this.DelegateFunctionNamePostfix);
            sb.Append(getFunction);
        }

        private void CsharpApiToCApiType(StringBuilder sb, string typename, string varname)
        {
            // SWIFT_API CONSTRAINT_PARAMETERIZER_PTR CreateMuskingumConstraint(HYPERCUBE_PTR hypercubeParameterizer, double deltaT, const char* paramNameK, const char* paramNameX, MODEL_SIMULATION_PTR simulation);
            //public static INativeParameterizer CreateMuskingumConstraint_cs(INativeParameterizer parameterizer, double deltaT, string paramNameK, string paramNameX, IModelSimulation simulation)
            //{
            //    IntPtr p = MsDotnetNativeApi.NativeSwiftLib.GetFunction<CreateMuskingumConstraint_csdelegate>("CreateMuskingumConstraint")
            //            (parameterizer.GetHandle(), deltaT, paramNameK, paramNameX, (simulation == null ? IntPtr.Zero : simulation.GetHandle()));
            //    return createParamaterizerWrapper(p);
            //}

            // If this is a pointer, take precedence on known types.\

            TransientArgumentConversion t = FindTransientArgConversion(typename, varname);
            if (t != null)
                sb.Append(t.LocalVarname);
            else if (IsPointer(typename)) // HYPERCUBE_PTR
                ConvertIntPtrToCapi(sb, typename, varname);
            else if (IsKnownType(typename))
                sb.Append(AddAs(typename, varname));
            else
                sb.Append(varname);
        }

        protected override void ApiTypeToCsharpApi(StringBuilder sb, string typename)
        {
            if (IsKnownType(typename))
                sb.Append(AnsiCToCsharpTypes(typename));
            else if (IsPointer(typename))
                sb.Append(WrapPointerType(typename)); // XPtr<ModelRunner>
            else
                sb.Append(AnsiCToCsharpTypes(typename));
        }

        private string WrapPointerType(string cApiType, string varname = "", bool instance = false)
        {
            // ModelRunner* becomes  IntPtr
            // const void* becomes  IntPtr
            // CONSTRAINT_PARAMETERIZER_PTR  becomes IntPtr
            string res = "IntPtr";
            if (instance)
            {
                // However char* becomes string
                if (IsCharPtr(cApiType))
                    res = "InteropHelper.PtrToStringAnsi(" + varname + ", true)";
                else if (IsKnownSwiftType(cApiType))
                    // TODO: 
                    //var m = new ModelSimulation(ptr);
                    //return (CreateProxies ? new ModelSimulationProxy(m) : (IModelSimulation)m);
                    res = "createWrapper" + instanceTypeMap[cApiType] + "(" + varname + ")";
                else
                    res = "new " + cApiType + "(" + varname + ")";
                //else
                //    res = res + "(" + varname + ")";
            }
            return res;
        }

        // TODO: refactor - minor duplicate
        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                AddBodyLine(sb, "var x = " + CsharpWrap(funcDef.TypeName, ReturnedValueVarname));
                AddBodyLine(sb, "return x");
            }
        }

        private void ConvertIntPtrToCapi(StringBuilder sb, string typename, string varname)
        {
            // C API  HYPERCUBE_PTR parameterizer
            // CSHARP:  INativeParameterizer parameterizer
            //CheckedDangerousGetHandle(parameterizer)
            if (typename.EndsWith("char*")) // C API is char* ; would be a string in the PInvoke signature
                sb.Append(varname);
            else
                sb.Append("CheckedDangerousGetHandle(" + varname + ", \"" + varname + "\")");
        }

        private string AddAs(string typename, string varname)
        {
            // the .NET interop layer and the use of dynamic interop should take care of conversions without casts
            // return ("(" + typename + ")" + varname);
            return varname;
        }

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            return createWrappingFunctionSignature(sb, funcAndArgs, null, ApiArgToCsharp, FunctionNamePostfix);
        }

        private bool IsKnownSwiftType(string cApiType)
        {
            return IsKnownType(cApiType) && cApiType.EndsWith("PTR");
        }

        private string CsharpWrap(string typename, string varname)
        {
            //return string.Format("TodoCsharpWrap({0}, {1})", typename, varname);
            if (IsPointer(typename))
                return (WrapPointerType(typename, varname, true));
            else if (IsKnownType(typename))
                return WrapAsCsharpType(typename, varname);
            else
                return WrapAsCsharpType(typename, varname);
        }

        private string WrapAsCsharpType(string typename, string varname)
        {
            if (typename == "double" ||
                typename == "int" ||
                typename == "bool")
                return varname;
            return AnsiCToCsharpTypes(typename) + "(" + varname + ")";
        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                IsMatchFunc = StringHelper.ReturnsCharPP,
                ApiArgToWrappingLang = ApiArgToCsharp,
                ApiCallArgument = ApiCallArgument,
                TransientArgsCreation = TransientArgsCreation,
                TransientArgsCleanup = TransientArgsCleanup,
                FunctionNamePostfix = this.FunctionNamePostfix,

                Template = @"string[] %FUNCTION%_cs(%WRAPARGS%)
{
    IntPtr size = InteropHelper.AllocHGlobal<int>();
%TRANSARGS%    IntPtr result = " + ClassName + @".NativeSwiftLib.GetFunction<%FUNCTION%_csdelegate>(" +
QuotedString("%FUNCTION%") +
@")(%ARGS%, size);
%CLEANTRANSARGS%    int n = InteropHelper.Read<int>(size, true);
    return InteropHelper.GlobalAnsiToArrayString(result, n, true);
}
"
            };

            return cw;
        }

    }

    public class CsharpDelegatesApiWrapperGenerator : BaseCsharpApiWrapperGenerator
    {
        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            StringBuilder sb = new StringBuilder();
            //private delegate IntPtr CreateMuskingumConstraint_csdelegate(IntPtr parameterizer, double deltaT, string paramNameK, string paramNameX, IntPtr simulation);
            if (!createDelegateDeclaration(sb, funcAndArgs)) return line;

            return sb.ToString();
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            // should not be called for delegate signature declaration. Do nothing.
        }

        public CsharpDelegatesApiWrapperGenerator()
        {
            DeclarationOnly = true;


            SetTypeMap("MODEL_SIMULATION_PTR", "IntPtr");
            SetTypeMap("SIMULATION_BASE_PTR", "IntPtr");
            SetTypeMap("MEMORY_STATES_PTR", "IntPtr");
            SetTypeMap("OBJECTIVE_EVALUATOR_PTR", "IntPtr");
            SetTypeMap("HYPERCUBE_PTR", "IntPtr");
            SetTypeMap("PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("COMPOSITE_PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("CONSTRAINT_PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("SCALING_PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("STATE_INIT_PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("TRANSFORM_PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("SUBAREAS_SCALING_PARAMETERIZER_PTR", "IntPtr");
            SetTypeMap("STATE_INITIALIZER_PTR", "IntPtr");
            SetTypeMap("TIME_SERIES_PTR", "IntPtr");
            SetTypeMap("ENSEMBLE_DATA_SET_PTR", "IntPtr");
            SetTypeMap("ENSEMBLE_FORECAST_TIME_SERIES_PTR", "IntPtr");
            SetTypeMap("ENSEMBLE_TIME_SERIES_PTR", "IntPtr");
            SetTypeMap("ENSEMBLE_PTR_TIME_SERIES_PTR", "IntPtr");
            SetTypeMap("TIME_SERIES_PROVIDER_PTR", "IntPtr");
            SetTypeMap("ENSEMBLE_FORECAST_SIMULATION_PTR", "IntPtr");
            SetTypeMap("ENSEMBLE_SIMULATION_PTR", "IntPtr");
            SetTypeMap("CANDIDATE_FACTORY_SEED_WILA_PTR", "IntPtr");
            SetTypeMap("OBJECTIVE_EVALUATOR_WILA_PTR", "IntPtr");
            SetTypeMap("OBJECTIVE_SCORES_WILA_PTR", "IntPtr");
            SetTypeMap("HYPERCUBE_WILA_PTR", "IntPtr");
            SetTypeMap("SCE_TERMINATION_CONDITION_WILA_PTR", "IntPtr");
            SetTypeMap("OPTIMIZER_PTR", "IntPtr");
            SetTypeMap("VEC_OBJECTIVE_SCORES_PTR", "IntPtr");
            SetTypeMap("VOID_PTR_PROVIDER_PTR", "IntPtr");

            // The delegate declaration will pass an IntPtr for char**
            SetTypeMap("char**", "IntPtr");

        }

        private bool createDelegateDeclaration(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            sb.Append(UniformIndentation);
            sb.Append("private delegate ");
            // we have to use a custom call below, not the parent, because of char* needing different treatment... KLUDGE
            bool result = createCsharpWrappingFunctionSignature(sb, funcAndArgs, ApiArgToCsharpDelegateArg, DelegateFunctionNamePostfix);
            sb.Append(StatementSep);
            sb.Append(EnvNewLine);
            return result;
        }

        protected bool createCsharpWrappingFunctionSignature(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argumentConverterFunction, string functionNamePostfix)
        {
            string funcDef = funcAndArgs.Function + functionNamePostfix;
            if (!StringHelper.ParseTypeAndName(sb, funcDef, ReturnApiArgToCsharpDelegateArg)) return false;
            return AddFunctionArgs(sb, funcAndArgs, argumentConverterFunction);
        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                IsMatchFunc = StringHelper.ReturnsCharPP,
                ApiArgToWrappingLang = ApiArgToCsharp,
                ApiCallArgument = ApiCallArgument,
                FunctionNamePostfix = this.FunctionNamePostfix,

                Template = UniformIndentation + @"private delegate IntPtr %FUNCTION%_csdelegate(%WRAPARGS%, IntPtr size)"
            };

            return cw;
        }
        protected override void ApiTypeToCsharpApi(StringBuilder sb, string typename)
        {
            if (IsKnownType(typename))
                sb.Append(AnsiCToCsharpTypes(typename));
            //else if (IsPointer(typename))
            //    sb.Append(WrapPointerType(typename)); // XPtr<ModelRunner>
            else
                sb.Append(AnsiCToCsharpTypes(typename));
        }

        private void ApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            // Do nothing - delegates are declaration only
        }
    }
}
