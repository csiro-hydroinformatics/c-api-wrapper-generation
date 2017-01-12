using System.Text;

namespace ApiWrapperGenerator
{
    public class CppApiWrapperGenerator : BaseApiConverter, IApiConverter
    {
        public CppApiWrapperGenerator()
        {
            AssignmentSymbol = "=";
            ReturnedValueVarname = "result";
            FunctionNamePostfix = "_cpp";
            //OpaquePointers = false;
            DeclarationOnly = false;
            //AddRcppExport = true;
            NewLineString = StringHelper.NewLineString;

            SetTypeMap("void", "void");
            SetTypeMap("int", "int");
            //SetTypeMap("int*", "int*");
            SetTypeMap("char**", "std::vector<std::string>&");
            SetTypeMap("char*", "std::string");
            SetTypeMap("char", "std::string");
            SetTypeMap("double", "double");
            SetTypeMap("double*", "const std::vector<double>&");
            SetTypeMap("double**", "const std::vector<std::vector<double>>&");
            SetTypeMap("bool", "bool");
            SetTypeMap("const char", "const char");
            SetTypeMap("const int", "const int");
            SetTypeMap("const double", "const double");
            SetTypeMap("const char*", "const std::string");
            SetTypeMap("const int*", "const std::vector<int>&");
            SetTypeMap("const double*", "const std::vector<double>&");

            OpaquePointerClassName = "OpaquePointer";
            PrependOutputFile = "// This file was GENERATED\n//Do NOT modify it manually, as you are very likely to lose work\n\n";

        }

        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            //SWIFT_API MODEL_SIMULATION_PTR CloneModel(MODEL_SIMULATION_PTR src);
            //SWIFT_API char** CheckSimulationErrors(MODEL_SIMULATION_PTR simulation, int* size);
            // And as an output we want for instance (if using opaque pointers).
            // 
            // OpaquePointer* CloneModel_cpp(OpaquePointer* src)
            // {
            //     return new OpaquePointer(CloneModel(src->Get()));
            // }
            // std::vector<std::string> CheckSimulationErrors(OpaquePointer simulation);
            // {
            //   int size;
            //   char** names = CheckSimulationErrors(simulation->Get(), &size);
            //   return toVectorCleanup(names, size);
            // }

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
            CppApiToCApiType(sb, typeAndName.TypeName, typeAndName.VarName);
        }

        private void CppApiToCApiType(StringBuilder sb, string typename, string varname)
        {
            //void SetErrorCorrectionModel_R(OpaquePointer* src, const std::string& newModelId, const std::string& elementId, int length, int seed)
            //{
            //    SetErrorCorrectionModel(src->Get(), newModelId.c_str(), elementId.c_str(), length, seed);
            //}

            if (IsKnownType(typename))
                sb.Append(AddAs(typename, varname));
            else if (IsPointer(typename))
                ConvertPointerTypeToCapi(sb, typename, varname);
            else
                sb.Append(varname);
        }

        private string AddAs(string typename, string varname)
        {
            if (typename.EndsWith("char*")) // C API is char*, so cpp is a std::string
                return varname + ".c_str()";
            return ("(" + typename + ")" + varname);
        }

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            return createWrappingFunctionSignature(sb, funcAndArgs, ApiArgToRcpp, FunctionNamePostfix);
        }

        private void ApiArgToRcpp(StringBuilder sb, TypeAndName typeAndName)
        {
            var rt = typeAndName.TypeName;
            ApiTypeToCppApi(sb, rt);
            sb.Append(" ");
            sb.Append(typeAndName.VarName);
        }

        private void ApiTypeToCppApi(StringBuilder sb, string typename)
        {
            if (IsKnownType(typename))
                sb.Append(AnsiCToCppTypes(typename));
            else if (IsPointer(typename))
                sb.Append(AsOpaquePtr(typename)); // XPtr<ModelRunner>
            else
                sb.Append(AnsiCToCppTypes(typename));
        }

        public string OpaquePointerClassName { get; set; }

        private string AsOpaquePtr(string typePtr, string varname = "", bool instance = false) // ModelRunner* becomes   XPtr<ModelRunner>
        {
            string res = OpaquePointerClassName + CPtr;
            if (instance)
            {
                //if (OpaquePointers)
                res = "new " + OpaquePointerClassName + "(" + varname + ")";
                //else
                //    res = res + "(" + varname + ")";
            }
            return res;
        }

        private string AnsiCToCppTypes(string rt)
        {
            return DefaultAnsiCToWrapperType(rt);
        }

        // TODO: refactor - minor duplicate
        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                sb.Append("    auto x = " + CppWrap(funcDef.TypeName, ReturnedValueVarname) + StatementSep + NewLineString);
                if (funcDef.TypeName == "char*")
                    sb.Append("    DeleteAnsiString(" + ReturnedValueVarname + ");" + NewLineString);
                sb.Append("    return x;");
            }
        }

        private string CppWrap(string typename, string varname)
        {
            //return string.Format("TodoCppWrap({0}, {1})", typename, varname);
            if (IsKnownType(typename))
                return WrapAsCppType(typename, varname);
            else if (IsPointer(typename))
                return (AsOpaquePtr(typename, varname, true));
            else
                return WrapAsCppType(typename, varname);
        }

        private string WrapAsCppType(string typename, string varname)
        {
            if (typename == "double" ||
                typename == "int" ||
                typename == "bool")
                return varname;
            return AnsiCToCppTypes(typename) + "(" + varname + ")";
        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                IsMatchFunc = StringHelper.ReturnsCharPP,
                ApiArgToWrappingLang = ApiArgToRcpp,
                ApiCallArgument = ApiCallArgument,
                FunctionNamePostfix = this.FunctionNamePostfix,
                Template = @"
std::vector<std::string> %WRAPFUNCTION%(%WRAPARGS%)
{
	int size; 
	char** names = %FUNCTION%(%ARGS% &size);
	return toVectorCleanup(names, size);
}
"
            };

            return cw;
        }

    }
}
