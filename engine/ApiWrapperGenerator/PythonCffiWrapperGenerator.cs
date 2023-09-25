using System;
using System.Collections.Generic;
using System.Text;

namespace ApiWrapperGenerator
{
    /// <summary>
    /// Converts C API functions declarations to 
    /// Python code (using CFFI) wrapping/unwrapping external pointers.
    /// </summary>
    public class PythonCffiWrapperGenerator : BaseApiConverter
    {

        static PythonCffiWrapperGenerator()
        {
            TypeAndName.ReservedWords.Add("lambda");
        }

        public PythonCffiWrapperGenerator()
        {

            ApiCallPrefix = "mynativelib.";

            AssignmentSymbol = "=";
            ReturnedValueVarname = "result";
            Indentation = "    ";
            FunctionBodyOpenDelimiter = "";
            FunctionBodyCloseDelimiter = "";
            PyDocstringStartMarker = "";
            PyDocExportFunctions = true;

            // See https://pypi.org/project/refcount/
            CreateXptrObjRefFunction = "custom_wrap_cffi_native_handle"; // because needs a custom callback del function.
            GetXptrFromObjRefFunction = "unwrap_cffi_native_handle";
            PyDocstringParameterTag = "";
            PyDocstringExportTag = "";

            StatementSep = "";

            ClearCustomWrappers();
            CustomFunctionWrapperImpl cw = ReturnsCharPtrPtrWrapper();
            AddCustomWrapper(cw);

            FunctionWrappers = ""; 

            GeneratePyDocstringDoc = true;
            PyDocstringDocPostamble = string.Empty;

            SetTypeMap("void", "None");
            SetTypeMap("int", "int");
            SetTypeMap("int*", "np.ndarray");
            SetTypeMap("char**", "List[str]");
            SetTypeMap("char*", "str");
            SetTypeMap("char", "char");
            SetTypeMap("double", "float");
            SetTypeMap("double*", "np.ndarray");
            SetTypeMap("double**", "np.ndarray");
            SetTypeMap("bool", "bool");
            // SetTypeMap("const char", "const char");
            // SetTypeMap("const int", "int");
            // SetTypeMap("const double", "NumericVector");
            // SetTypeMap("const char*", "str");
            // SetTypeMap("const int*", "IntegerVector");
            // SetTypeMap("const double*", "NumericVector");


            //SetTransientArgConversion(".*", "",
            //    "C_ARGNAME = " + GetXptrFromObjRefFunction + @"(RCPP_ARGNAME)" + StatementSep, //    x <- getSwiftXptr(x);
            //    ""); // no cleanup

            // termination_elems_c = [FFI_.new("char[]", as_bytes(x)) for x in termination_elements]
            // which I want as:
            // termination_elems_c = to_c_char_ptrptr(termination_elements)
            SetTransientArgConversion(
                "char**",
                "_charpp",
                // "IntPtr elementIds_charpp = InteropHelper.ArrayStringToHGlobalAnsi(elementIds);"
                // "IntPtr C_ARGNAME = InteropHelper.ArrayStringToHGlobalAnsi(RCPP_ARGNAME);"
                "C_ARGNAME = to_c_char_ptrptr(RCPP_ARGNAME)",
                ""); // no cleanup?


        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            return ReturnsVectorWrapper(StringHelper.ReturnsCharPP, "char**", "List", "charp_array_to_py");
        }

        public CustomFunctionWrapperImpl ReturnsDoublePtrWrapper()
        {
            return ReturnsVectorWrapper(StringHelper.ReturnsDoublePtr, "double**", "List", "numeric_matrix_to_py");
        }

        public CustomFunctionWrapperImpl ReturnsVectorWrapper(System.Func<string, bool> matchFun, string apitype, 
            string pytype, string convertingFunc)
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                StatementSep = this.StatementSep,
                IsMatchFunc = matchFun,
                ApiArgToWrappingLang = ApiArgToPyFunctionArgument,
                ApiCallArgument = this.ApiCallArgument,
                TransientArgsCreation = this.TransientArgsCreation,
                TransientArgsCleanup = TransientArgsCleanup,
                FunctionNamePostfix = this.FunctionNamePostfix,
                CalledFunctionNamePrefix = this.ApiCallPrefix,
                CalledFunctionNamePostfix = this.ApiCallPostfix,
                ApiSignatureToDocString = this.ApiSignatureToBasicPyDocstringString,
                Template = @"
def _%CFUNCTION%_native(%CARGSNAMES%, size):
    return %FUNCTION%(%CARGSNAMES%, size)

def %WRAPFUNCTION%(%WRAPARGS%):
%WRAPFUNCTIONDOCSTRING%
%TRANSARGS%
    size = marshal.new_int_scalar_ptr()
    values = _%CFUNCTION%_native(%ARGS%, size)
%CLEANTRANSARGS%
    result = " + convertingFunc + @"(values, size[0], True)
    return result
"
            };
            return cw;
        }

        private void createFfiApiFunctionCall(StringBuilder sb, TypeAndName funcDef)
        {
            sb.Append(ApiCallPrefix + funcDef.VarName + ApiCallPostfix);
        }

        private bool createNativeWrapFuncBody(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc)
        {
            Dictionary<string, TransientArgumentConversion> tConv = new Dictionary<string, TransientArgumentConversion>();
            var funcDef = GetTypeAndName(funcAndArgs.Function);
            bool returnsVal = FunctionReturnsValue(funcDef);
            bool ok = CreateFunctionCall(sb, funcAndArgs, argFunc, this.createFfiApiFunctionCall, tConv, funcDef, returnsVal);
            if (!ok) return false;
            CreateBodyReturnValueNative(sb, funcDef, returnsVal);
            return true;
        }

        protected string createNativeWrappingFunctionBody(string line, FuncAndArgs funcAndArgs, StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc)
        {
            string result;
            sb.Append(BodyLineOpenFunctionDelimiter);
            // AddInFunctionDocString(sb, funcAndArgs);
            bool ok = createNativeWrapFuncBody(sb, funcAndArgs, argFunc);
            sb.Append(BodyLineCloseFunctionDelimiter);
            if (!ok)
                result = line;
            else
                result = sb.ToString();
            return result;
        }

        private string checkedNativeCall(string line, FuncAndArgs funcAndArgs)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(FunctionWrappers))
                //@check_exceptions
                sb.Append(FunctionWrappers + EnvNewLine);
            if (!createNakedWrapFuncSignature(sb, funcAndArgs)) return line;
            string result = "";
            result = createNativeWrappingFunctionBody(line, funcAndArgs, sb, NakedApiCallArgument);
            return result;
        }

        protected override void CreateApiFunctionCallFunction(StringBuilder sb, TypeAndName funcDef)
        {
            sb.Append( "_" + funcDef.VarName + "_native");
        }

        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            var sb = new StringBuilder();
            sb.Append(checkedNativeCall(line, funcAndArgs));
            if (!createTypedWrapFuncSignature(sb, funcAndArgs)) return line;
            string result = "";
            result = createWrappingFunctionBody(line, funcAndArgs, sb, ApiCallArgument);
            return result;
        }

        protected override void AddInFunctionDocString(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            if (GeneratePyDocstringDoc)
            {
                CreateWrapFuncPyDoc(sb, funcAndArgs);
            }
        }

        private void ApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            PythonApiToCApiType(sb, typeAndName.TypeName, typeAndName.VarName);
        }

        private void NakedApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName);
        }

        private void PythonApiToCApiType(StringBuilder sb, string typename, string varname)
        {
            TransientArgumentConversion t = FindTransientArgConversion(typename, varname);
            if (t != null)
            {
                // All transient arguments in python Cffi will be wrapped in a class, to keep alive all native resources; we need to pass the cffi pointer however
                // TODO: generalise
                string apiCallUnwrap = t.IsPointer ? ".ptr" : ".obj";
                sb.Append(t.LocalVarname + apiCallUnwrap); // Call with the transient variable name e.g. argname_char_pp
            }
            // If this is a pointer, take precedence on known types.\
            // else if (IsPointer(typename)) // HYPERCUBE_PTR
            //     ConvertIntPtrToCapi(sb, typename, varname);
            // else if (IsKnownType(typename))
            //     sb.Append(AddAs(typename, varname));
            else
                sb.Append(varname);
        }


        public bool GeneratePyDocstringDoc { get; set; }
        public string PyDocstringDocPostamble { get; set; }
        public string PyDocstringExportTag { get; set; }
        public string PyDocstringParameterTag { get; set; }
        public string PyDocstringStartMarker { get; set; }
        public bool PyDocExportFunctions { get; set; }
        public string FunctionWrappers { get; set; }
        public void AddPyDocLine (StringBuilder sb, string lineTxt = "")
        {
            AddLine(sb, Indentation + PyDocstringStartMarker + lineTxt);
        }

        public void AddPyDocstringStart(StringBuilder sb)
        {
            AddNoNewLine(sb, Indentation + "\"\"\"");
        }

        public void AddPyDocstringEnd(StringBuilder sb)
        {
            AddLine(sb, Indentation + "\"\"\"");
        }

        private string PythonType(string apiType)
        {
            return TypeMap.ContainsKey(apiType) ? TypeMap[apiType] : "Any";
        }

        public bool CreateWrapFuncPyDoc(StringBuilder sb, FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            /*
    """[summary]

    Args:
        simulation ([type]): [description]
        variableIdentifier ([type]): [description]

    Returns:
        [type]: [description]
    """    
             */
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcName = funcDecl.VarName + FunctionNamePostfix;
            AddPyDocstringStart(sb);
            sb.Append(funcName + EnvNewLine);
            AddPyDocLine(sb);
            AddPyDocLine(sb, funcName + ": generated wrapper function for API function " + funcDecl.VarName);
            AddPyDocLine(sb);
            if (paramDocs)
            {
                AddPyDocLine(sb, "Args:");
                var funcArgs = GetFuncArguments(funcAndArgs);
                for (int i = 0; i < funcArgs.Length; i++)
                {
                    var v = GetTypeAndName(funcArgs[i]);
                    // AddPyDocLine(sb, PyDocstringParameterTag + "" + v.VarName + " Python type equivalent for C++ type " + v.TypeName);
                    AddPyDocLine(sb, Indentation + string.Format("{0} ({1}): {2}", v.VarName, PythonType(v.TypeName), v.VarName));
                }
                var funcDef = GetTypeAndName(funcAndArgs.Function);
                bool returnsVal = FunctionReturnsValue(funcDef);
                if (returnsVal)
                {
                    AddPyDocLine(sb);
                    AddPyDocLine(sb, "Returns:");
                    AddPyDocLine(sb, Indentation + string.Format("({0}): {1}", PythonType(funcDef.TypeName), "returned result"));
                }
            }
            if(PyDocExportFunctions)
                AddPyDocLine( sb, PyDocstringExportTag);
            AddPyDocstringEnd(sb);
            return true;
        }

        public string ApiSignatureToBasicPyDocstringString(FuncAndArgs funcAndArgs)
        {
            StringBuilder sb = new StringBuilder();
            CreateWrapFuncPyDoc(sb, funcAndArgs, false);
            return sb.ToString();
        }

        public string ApiSignatureToPyDocstringString(FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            StringBuilder sb = new StringBuilder();
            CreateWrapFuncPyDoc(sb, funcAndArgs, paramDocs);
            return sb.ToString();
        }

        private bool createTypedWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            // From:
            // SWIFT_API MODEL_SIMULATION_PTR SubsetModel(MODEL_SIMULATION_PTR simulation, const char* elementName, bool selectNetworkAboveElement, bool includeElementInSelection, bool invertSelection, char** terminationElements, int terminationElementsLength);
            // To:
            //def swift_subset_model(simulation, elementName, selectNetworkAboveElement, includeElementInSelection, invertSelection, terminationElements, terminationElementsLength):
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcDef = "def " + funcDecl.VarName + FunctionNamePostfix;
            sb.Append(funcDef);
            bool r = AddFunctionArgs(sb, funcAndArgs, ApiArgToPyFunctionArgument);
            sb.Append(" -> " + PythonType(funcDecl.TypeName));
            sb.Append(":");
            return r;
        }

        /// <summary>
        /// Defines the signature of a thin low level wrapper call to the native function
        /// </summary>
        private bool createNakedWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            // From:
            // SWIFT_API MODEL_SIMULATION_PTR SubsetModel(MODEL_SIMULATION_PTR simulation, const char* elementName, bool selectNetworkAboveElement, bool includeElementInSelection, bool invertSelection, char** terminationElements, int terminationElementsLength);
            // To e.g.:
            // @checked_errors
            //def SubsetModel_native(simulation, elementName, selectNetworkAboveElement, includeElementInSelection, invertSelection, terminationElements, terminationElementsLength):
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcDef = "def _" + funcDecl.VarName + "_native";
            sb.Append(funcDef);
            bool r = AddFunctionArgs(sb, funcAndArgs, ApiArgToUntypedPyFunctionArgument);
            // sb.Append(" -> " + PythonType(funcDecl.TypeName));
            sb.Append(":");
            return r;
        }

        private void ApiArgToPyFunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName + ":" + PythonType(typeAndName.TypeName));
        }

        private void ApiArgToUntypedPyFunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName);
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                string s = ("return " + PyWrap(funcDef.TypeName, ReturnedValueVarname));
                AddBodyLine(sb, s);
                // AddBodyLine(sb, "return x_res");
            }
        }

        private void CreateBodyReturnValueNative(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                string s = ("return " + ReturnedValueVarname);
                AddBodyLine(sb, s);
                // AddBodyLine(sb, "return x_res");
            }
        }

        private string PyWrap(string typename, string varname)
        {
            if (IsKnownType(typename)) // known types may include struct pointers with custom conv.
                return WrapAsPyType(typename, varname);
            else if (IsPointer(typename)) // otherwise fallback on opaque pointers.
                return (AsOpaquePtr(typename, varname, true));
            else
                return WrapAsPyType(typename, varname);
        }

        private string WrapAsPyType(string typename, string varname)
        {
            if (typename == "double" ||
                typename == "int" ||
                typename == "bool")
                return varname;
            var c = FindReturnedConverter(typename);
            if (c != null)
                return c.Apply(varname);
            else
                return varname;
        }
        private string AsOpaquePtr(string typename, string varname = "", bool instance = false) // ModelRunner* becomes   XPtr<ModelRunner>
        {
            return CreateXptrObjRefFunction + @"(" + ReturnedValueVarname + ", '" + typename + "')";
        }
    }
}
