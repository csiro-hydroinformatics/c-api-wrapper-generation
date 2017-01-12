using System.Text;

namespace ApiWrapperGenerator
{
    /// <summary>
    /// Converts C API functions declarations to 
    /// R code wrapping/unwrapping external pointers.
    /// </summary>
    public class RXptrWrapperGenerator : BaseApiConverter, IApiConverter
    {

        public RXptrWrapperGenerator()
        {
            AssignmentSymbol = "<-";
            ReturnedValueVarname = "result";
            Indentation = "  ";
            RoxygenStartMarker = "#'";
            RoxyExportFunctions = true;
            RoxygenParameterTag = "@param";
            RoxygenExportTag = "@export";
            StatementSep = "";

            ClearCustomWrappers();
            CustomFunctionWrapperImpl cw = ReturnsCharPtrPtrWrapper();
            AddCustomWrapper(cw);

            GenerateRoxygenDoc = true;
            RoxygenDocPostamble = string.Empty;

            SetTransientArgConversion(".*", "",
                "C_ARGNAME <- getSwiftXptr(RCPP_ARGNAME)" + StatementSep, //    x <- getSwiftXptr(x);
                ""); // no cleanup

        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                StatementSep = this.StatementSep,
                IsMatchFunc = StringHelper.ReturnsCharPP,
                ApiArgToWrappingLang = ApiArgToRfunctionArgument,
                ApiCallArgument = this.ApiCallArgument,
                TransientArgsCreation = this.TransientArgsCreation,
                FunctionNamePostfix = this.FunctionNamePostfix,
                CalledFunctionNamePostfix = this.ApiCallPostfix,
                ApiSignatureToDocString = this.ApiSignatureToBasicRoxygenString,
                Template = @"
%WRAPFUNCTIONDOCSTRING%
%WRAPFUNCTION% <- function(%WRAPARGS%)
{
    %TRANSARGS%
    result <- %FUNCTION%(%WRAPARGS%);
    return(mkSwiftObjRef(result,'char**'))
}
"
            };
            return cw;
        }

        /*

        SWIFT_API OBJECTIVE_EVALUATOR_PTR CreateObjectiveCalculator(MODEL_SIMULATION_PTR modelInstance, char* obsVarId, double * observations,
            int arrayLength, MarshaledDateTime start, char* statisticId);

        CreateObjectiveCalculator_R <- function(modelInstance, obsVarId, observations, arrayLength, start, statisticId) {
            .Call('swift_CreateObjectiveCalculator_R', PACKAGE = 'swift', modelInstance, obsVarId, observations, arrayLength, start, statisticId)
        }

        And we want to generate something like:

        CreateObjectiveCalculator_R_wrap <- function(modelInstance, obsVarId, observations, arrayLength, start, statisticId) {
            modelInstance_xptr <- getSwiftXptr(modelInstance)
            xptr <- CreateObjectiveCalculator_R(modelInstance_xptr, obsVarId, observations, arrayLength, start, statisticId)
            return(mkSwiftObjRef(xptr))
        }

        SWIFT_API_FUNCNAME_R_wrap <- function(modelInstance, obsVarId, observations, arrayLength, start, statisticId) {
            modelInstance_xptr <- getSwiftXptr(modelInstance)
            xptr <- SWIFT_API_FUNCNAME_R(modelInstance_xptr, obsVarId, observations, arrayLength, start, statisticId)
            return(mkSwiftObjRef(xptr))
        }

}

*/
        public override string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs)
        {
            var sb = new StringBuilder();
            if (GenerateRoxygenDoc)
            {
                if (!CreateWrapFuncRoxydoc(sb, funcAndArgs))
                    return line;
            }
            if (!createWrapFuncSignature(sb, funcAndArgs)) return line;
            string result = "";
            result = createWrappingFunctionBody(line, funcAndArgs, sb, ApiCallArgument);
            return result;
        }

        private void ApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            //    xptr <- SWIFT_API_FUNCNAME_R(modelInstance_xptr, obsVarId, observations, arrayLength, start, statisticId)
            // in context:
            //SWIFT_API_FUNCNAME_R_wrap < -function(modelInstance, obsVarId, observations, arrayLength, start, statisticId) {
            //    modelInstance_xptr < -getSwiftXptr(modelInstance)
            //    xptr <- SWIFT_API_FUNCNAME_R(modelInstance_xptr, obsVarId, observations, arrayLength, start, statisticId)
            //    return (mkSwiftObjRef(xptr))
            //}
            sb.Append(typeAndName.VarName);
        }

        public bool GenerateRoxygenDoc { get; set; }

        public string RoxygenDocPostamble { get; set; }

        public string RoxygenExportTag { get; set; }
        public string RoxygenParameterTag { get; set; }
        public string RoxygenStartMarker { get; set; }
        public bool RoxyExportFunctions { get; set; }

        public void AddRoxyLine (StringBuilder sb, string lineTxt = "")
        {
            AddLine(sb, RoxygenStartMarker + lineTxt);
        }

        public bool CreateWrapFuncRoxydoc(StringBuilder sb, FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcName = funcDecl.VarName + FunctionNamePostfix;
            AddRoxyLine(sb, " " + funcName);
            AddRoxyLine(sb);
            AddRoxyLine(sb, " " + funcName + " Wrapper function for " + funcDecl.VarName);
            AddRoxyLine(sb);
            if (paramDocs)
            {
                var funcArgs = GetFuncArguments(funcAndArgs);
                for (int i = 0; i < funcArgs.Length; i++)
                {
                    var v = GetTypeAndName(funcArgs[i]);
                    AddRoxyLine(sb, " " + RoxygenParameterTag + " " + v.VarName + " R type equivalent for C++ type " + v.TypeName);
                }
            }
            if(RoxyExportFunctions)
                AddRoxyLine( sb, " " + RoxygenExportTag);
            return true;
        }

        public string ApiSignatureToBasicRoxygenString(FuncAndArgs funcAndArgs)
        {
            StringBuilder sb = new StringBuilder();
            CreateWrapFuncRoxydoc(sb, funcAndArgs, false);
            return sb.ToString();
        }

        public string ApiSignatureToRoxygenString(FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            StringBuilder sb = new StringBuilder();
            CreateWrapFuncRoxydoc(sb, funcAndArgs, paramDocs);
            return sb.ToString();
        }

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcDef = funcDecl.VarName + FunctionNamePostfix + " <- function";
            sb.Append(funcDef);
            bool r = AddFunctionArgs(sb, funcAndArgs, ApiArgToRfunctionArgument);
            sb.Append(' ');
            return r;
        }

        private void ApiArgToRfunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName);
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                AddBodyLine(sb, "return(mkSwiftObjRef(" + ReturnedValueVarname + ", '" + funcDef.TypeName + "'))");
            }
        }
    }
}
