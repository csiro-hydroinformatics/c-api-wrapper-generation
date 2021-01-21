using System.Text;

namespace ApiWrapperGenerator
{
    /// <summary>
    /// Converts C API functions declarations to 
    /// Python code (using CFFI) wrapping/unwrapping external pointers.
    /// </summary>
    public class PythonCffiWrapperGenerator : BaseApiConverter, IApiConverter
    {

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
            return ReturnsVectorWrapper(StringHelper.ReturnsCharPP);
        }

        public CustomFunctionWrapperImpl ReturnsDoublePtrWrapper()
        {
            return ReturnsVectorWrapper(StringHelper.ReturnsDoublePtr);
        }

        public CustomFunctionWrapperImpl ReturnsVectorWrapper(System.Func<string, bool> matchFun)
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                StatementSep = this.StatementSep,
                IsMatchFunc = matchFun,
                ApiArgToWrappingLang = ApiArgToPyFunctionArgument,
                ApiCallArgument = this.ApiCallArgument,
                TransientArgsCreation = this.TransientArgsCreation,
                FunctionNamePostfix = this.FunctionNamePostfix,
                CalledFunctionNamePrefix = this.ApiCallPrefix,
                CalledFunctionNamePostfix = this.ApiCallPostfix,
                ApiSignatureToDocString = this.ApiSignatureToBasicPyDocstringString,
                Template = @"
def %WRAPFUNCTION%(%WRAPARGS%):
%WRAPFUNCTIONDOCSTRING%
    %TRANSARGS%
    result = %FUNCTION%(%WRAPARGS%)
    return "+ CreateXptrObjRefFunction + @"(result,'dummytype')
"
            };
            return cw;
        }

        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(FunctionWrappers))
                //@convert_strings
                //@check_exceptions
                sb.Append(FunctionWrappers + NewLineString);
            if (!createWrapFuncSignature(sb, funcAndArgs)) return line;
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
//"""
//    :param str element_name
//    :param bool select_network_above_element
//    :param bool include_element_in_selection
//    :param bool invert_selection
//    :param list termination_elements: List of str.
//"""
//termination_elems_c = [FFI_.new("char[]", as_bytes(x)) for x in termination_elements]
//return LIB.SubsetModel(ptr, element_name, select_network_above_element,
//                        include_element_in_selection, invert_selection,
//                        termination_elems_c, len(termination_elements))
            sb.Append(typeAndName.VarName);
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
            sb.Append(funcName + NewLineString);
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

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
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

        private void ApiArgToPyFunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName + ":" + PythonType(typeAndName.TypeName));
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
            //return "TODO_" + typename + "(" + varname + ")";
        }
        private string AsOpaquePtr(string typename, string varname = "", bool instance = false) // ModelRunner* becomes   XPtr<ModelRunner>
        {
            return CreateXptrObjRefFunction + @"(" + ReturnedValueVarname + ", '" + typename + "')";
        }
    }
}
