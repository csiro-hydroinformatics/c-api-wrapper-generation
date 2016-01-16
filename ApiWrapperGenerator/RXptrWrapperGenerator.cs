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

            ClearCustomWrappers();
            CustomFunctionWrapperImpl cw = ReturnsCharPtrPtrWrapper();
            AddCustomWrapper(cw);

            GenerateRoxygenDoc = true;
            RoxygenDocPostamble = string.Empty;

        }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                IsMatchFunc = StringHelper.ReturnsCharPP,
                ApiArgToRcpp = ApiArgToRfunctionArgument,
                ApiCallArgument = this.ApiCallArgument,
                TransientArgsCreation = this.TransientArgCreation,
                FunctionNamePostfix = this.FunctionNamePostfix,
                CalledFunctionNamePostfix = this.ApiCallPostfix,
                Template = @"
#' docco
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
                if (!createWrapFuncRoxydoc(sb, funcAndArgs))
                    return line;
            }
            if (!createWrapFuncSignature(sb, funcAndArgs)) return line;
            string result = "";
            result = createWrappingFunctionBody(line, funcAndArgs, sb, ApiCallArgument);
            return result;
        }

        private void TransientArgCreation(StringBuilder sb, TypeAndName typeAndName)
        {
            //    x <- getSwiftXptr(x);
            sb.Append(typeAndName.VarName);
            sb.Append(" <- getSwiftXptr(");
            sb.Append(typeAndName.VarName);
            sb.Append(");");
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

        private bool createWrapFuncRoxydoc(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcName = funcDecl.VarName + FunctionNamePostfix;
            sb.AppendLine("#' " + funcName);
            sb.AppendLine("#' ");
            sb.AppendLine("#' " + funcName + " Wrapper function for " + funcDecl.VarName);
            sb.AppendLine("#' ");
            var funcArgs = GetFuncArguments(funcAndArgs);
            for (int i = 0; i < funcArgs.Length; i++)
            {
                var v = GetTypeAndName(funcArgs[i]);
                sb.Append("#' @param " + v.VarName + " R type equivalent for C++ type " + v.TypeName + NewLineString);
            }
            sb.AppendLine(RoxygenDocPostamble);
            return true;
        }

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcDef = funcDecl.VarName + FunctionNamePostfix + " <- function";
            sb.Append(funcDef);
            return AddFunctionArgs(sb, funcAndArgs, ApiArgToRfunctionArgument);
        }

        private void ApiArgToRfunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName);
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                sb.Append("    return(mkSwiftObjRef(" + ReturnedValueVarname + ",'" + funcDef.TypeName + "'))");
            }
        }

        protected override void AppendReturnedValueDeclaration(StringBuilder sb)
        {
            sb.Append(ReturnedValueVarname);
            sb.Append(" "); sb.Append(AssignmentSymbol); sb.Append(" ");
        }
    }
}
