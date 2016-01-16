using System.Text;

namespace ApiWrapperGenerator
{
    public class RcppGlueWrapperGenerator : BaseApiConverter, IApiConverter
    {

        protected override void AppendReturnedValueDeclaration(StringBuilder sb)
        {
            sb.Append("auto "); sb.Append(ReturnedValueVarname);
            sb.Append(" "); sb.Append(AssignmentSymbol); sb.Append(" ");
        }

        public RcppGlueWrapperGenerator()
        {
            AssignmentSymbol = "=";
            ReturnedValueVarname = "result";
            FunctionNamePostfix = "_R";
            OpaquePointers = false;
            DeclarationOnly = false;
            AddRcppExport = true;
            NewLineString = "\n";

            SetTypeMap("void", "void");
            SetTypeMap("int", "IntegerVector");
            SetTypeMap("int*", "IntegerVector");
            SetTypeMap("char**", "CharacterVector");
            SetTypeMap("char*", "CharacterVector");
            SetTypeMap("char", "CharacterVector");
            SetTypeMap("double", "NumericVector");
            SetTypeMap("double*", "NumericVector");
            SetTypeMap("double**", "NumericMatrix");
            SetTypeMap("bool", "LogicalVector");
            SetTypeMap("const char", "CharacterVector");
            SetTypeMap("const int", "IntegerVector");
            SetTypeMap("const double", "NumericVector");
            SetTypeMap("const char*", "CharacterVector");
            SetTypeMap("const int*", "IntegerVector");
            SetTypeMap("const double*", "NumericVector");

            OpaquePointerClassName = "OpaquePointer";
            PrependOutputFile = "// This file was GENERATED\n//Do NOT modify it manually, as you are very likely to lose work\n\n";

            ClearCustomWrappers();
            CustomFunctionWrapperImpl cw = ReturnsCharPtrPtrWrapper();
            AddCustomWrapper(cw);
        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            return new CustomFunctionWrapperImpl()
            {
                IsMatchFunc = StringHelper.ReturnsCharPP,
                ApiArgToRcpp = ApiArgToRcpp,
                ApiCallArgument = ApiCallArgument,
                FunctionNamePostfix = this.FunctionNamePostfix,
                Template = @"
// [[Rcpp::export]]
CharacterVector %WRAPFUNCTION%(%WRAPARGS%)
{
	int size; 
	char** names = %FUNCTION%(%ARGS% &size);
	return toVectorCleanup(names, size);
}
"
            };
        }

        public bool OpaquePointers { get; set; }

        public string OpaquePointerClassName { get; set; }

        public bool AddRcppExport { get; set; }

        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            // And as an output we want for instance (if using opaque pointers).
            // // [[Rcpp::export]]
            // XPtr<OpaquePointer> RcppCloneModel(XPtr<OpaquePointer> src)
            // {
            //     return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->Get())));
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

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            if (AddRcppExport)
                sb.Append("// [[Rcpp::export]]" + NewLineString);
            return createWrappingFunctionSignature(sb, funcAndArgs, ApiArgToRcpp);
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                sb.Append("    auto x = " + RcppWrap(funcDef.TypeName, ReturnedValueVarname) + StatementSep + NewLineString);
                if (funcDef.TypeName == "char*")
                    sb.Append("    DeleteAnsiString(" + ReturnedValueVarname + ");" + NewLineString);
                sb.Append("    return x;");
            }
        }

        private void ApiArgToRcpp(StringBuilder sb, TypeAndName typeAndName)
        {
            var rt = typeAndName.TypeName;
            ApiTypeToRcpp(sb, rt);
            sb.Append(" ");
            sb.Append(typeAndName.VarName);
        }

        private void ApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            RcppToApiType(sb, typeAndName.TypeName, typeAndName.VarName);
        }

        private void ApiTypeToRcpp(StringBuilder sb, string typename)
        {
            if (IsKnownType(typename))
                sb.Append(CppToRTypes(typename));
            else if (IsPointer(typename))
                sb.Append(createXPtr(typename)); // XPtr<ModelRunner>
            else
                sb.Append(CppToRTypes(typename));
        }

        private string RcppWrap(string typename, string varname)
        {
            if (IsKnownType(typename))
                return WrapAsRcppVector(typename, varname);
            else if (IsPointer(typename))
                return (createXPtr(typename, varname, true)); // XPtr<ModelRunner>(new OpaquePointer(varname))
            else
                return WrapAsRcppVector(typename, varname);
        }

        private string WrapAsRcppVector(string typename, string varname)
        {
            if (typename == "double" ||
                typename == "int" ||
                typename == "bool")
                return "Rcpp::wrap(" + varname + ")";
            return CppToRTypes(typename) + "(" + varname + ")";
        }

        private void RcppToApiType(StringBuilder sb, string typename, string varname)
        {

            //void SetErrorCorrectionModel_R(XPtr<OpaquePointer> src, CharacterVector newModelId, CharacterVector elementId, IntegerVector length, IntegerVector seed)
            //{
            //    SetErrorCorrectionModel(src->Get(), as<char*>(newModelId), as<char*>(elementId), as<int>(length), as<int>(seed));
            //}
            if (IsKnownType(typename))
                sb.Append(AddAs(typename, varname));
            else if (IsPointer(typename))
                ConvertPointerTypeToCapi(sb, typename, varname);
            else
                sb.Append(AddAs(typename, varname));
        }

        private string AddAs(string typename, string varname)
        {
            if (typename.EndsWith("char*"))
                return varname + "[0]";
            if (typename.EndsWith("double*") || typename.EndsWith("int*"))
                return "&(" + varname + "[0])";
            return ("as<" + typename + ">(" + varname + ")");
        }

        private string createXPtr(string typePtr, string varname = "", bool instance = false) // ModelRunner* becomes   XPtr<ModelRunner>
        {
            string res;
            if (OpaquePointers)
                res = "XPtr<" + OpaquePointerClassName + ">";
            else
                res = "XPtr<" + typePtr.Replace("*", "") + ">";
            if (instance)
            {
                if (OpaquePointers)
                    res = res + "(new " + OpaquePointerClassName + "(" + varname + "))";
                else
                    res = res + "(" + varname + ")";
            }
            return res;
        }

        private string CppToRTypes(string rt)
        {
            return DefaultAnsiCToWrapperType(rt);
        }
    }
}
