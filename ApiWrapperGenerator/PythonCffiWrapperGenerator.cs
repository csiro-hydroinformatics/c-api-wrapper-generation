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
            CreateXptrObjRefFunction = "cinterop.mkExternalObjRef";
            GetXptrFromObjRefFunction = "cinterop.getExternalXptr";

            ClearCustomWrappers();
            CustomFunctionWrapperImpl cw = ReturnsCharPtrPtrWrapper();
            AddCustomWrapper(cw);

            GeneratePyDocstringDoc = true;
            PyDocstringDocPostamble = string.Empty;

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

        private CustomFunctionWrapperImpl ReturnsVectorWrapper(System.Func<string, bool> matchFun)
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                StatementSep = this.StatementSep,
                IsMatchFunc = matchFun,
                ApiArgToWrappingLang = ApiArgToRfunctionArgument,
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
    return("+ CreateXptrObjRefFunction + @"(result,'dummytype'))
"
            };
            return cw;
        }

        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            var sb = new StringBuilder();
            // // TODO the @ things
            //@convert_strings
            //@check_exceptions
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

        public void AddPyDocLine (StringBuilder sb, string lineTxt = "")
        {
            AddLine(sb, Indentation + PyDocstringStartMarker + lineTxt);
        }

        public void AddPyDocstringStart(StringBuilder sb)
        {
            AddLine(sb, Indentation + "\"\"\"");
        }

        public void AddPyDocstringEnd(StringBuilder sb)
        {
            AddPyDocstringStart(sb);
        }

        public bool CreateWrapFuncPyDoc(StringBuilder sb, FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            /*
    """
        :param ptr series: Pointer to ENSEMBLE_FORECAST_TIME_SERIES_PTR
        :param int item_idx: Item index
        :param ndarray values: 2-dimensional array of values,
            1st dimension is lead time, second is ensemble member.
        :param datetime start: Start datetime for ``values``
        :param str freq: Frequency for ``values`` 'D' or 'H'.
    """
             */
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcName = funcDecl.VarName + FunctionNamePostfix;
            AddPyDocstringStart(sb);
            AddPyDocLine(sb, funcName);
            AddPyDocLine(sb);
            AddPyDocLine(sb, funcName + " Wrapper function for " + funcDecl.VarName);
            AddPyDocLine(sb);
            if (paramDocs)
            {
                var funcArgs = GetFuncArguments(funcAndArgs);
                for (int i = 0; i < funcArgs.Length; i++)
                {
                    var v = GetTypeAndName(funcArgs[i]);
                    AddPyDocLine(sb, PyDocstringParameterTag + " " + v.VarName + " Python type equivalent for C++ type " + v.TypeName);
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
            bool r = AddFunctionArgs(sb, funcAndArgs, ApiArgToRfunctionArgument);
            sb.Append(":");
            return r;
        }

        private void ApiArgToRfunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            // dynamic runtime type only for py:
            sb.Append(typeAndName.VarName);
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                AddBodyLine(sb, "return "+ CreateXptrObjRefFunction + @"(" + ReturnedValueVarname + ", '" + funcDef.TypeName + "')");
            }
        }
    }
}
