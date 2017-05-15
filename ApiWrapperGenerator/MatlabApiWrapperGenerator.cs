using System;
using System.Collections.Generic;
using System.Text;

namespace ApiWrapperGenerator
{
    public class MatlabApiWrapperGenerator : BaseApiConverter, IApiConverter
    {
        public MatlabApiWrapperGenerator()
        {
            FunctionNamePostfix = "_m";            
            AssignmentSymbol = "=";
            ReturnedValueVarname = "result";
            Indentation = "  ";
            FunctionBodyOpenDelimiter = "";
            FunctionBodyCloseDelimiter = "end";
            MatlabCommentMarker = "% ";
            //RoxyExportFunctions = true;
            MatlabInputParameterTag = "INPUT";
            //RoxygenExportTag = "@export";
            StatementSep = ";";
            CreateXptrObjRefFunction = "mkExternalObjRef";
            GetXptrFromObjRefFunction = "getExternalPtr";

            ClearCustomWrappers();
            CustomFunctionWrapperImpl cw = ReturnsCharPtrPtrWrapper();
            AddCustomWrapper(cw);

            GenerateFunctionDoc = false;
            //RoxygenDocPostamble = string.Empty;

            SetTransientArgConversion(".*", "",
                "C_ARGNAME = " + GetXptrFromObjRefFunction + @"(RCPP_ARGNAME)" + StatementSep, //    x_ptr = getSwiftXptr(x);
                ""); // no cleanup

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
                ApiArgToWrappingLang = ApiArgToMatlabfunctionArgument,
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
    return(" + CreateXptrObjRefFunction + @"(result,'dummytype'))
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
            if (GenerateFunctionDoc)
            {
                if (!CreateWrapFuncInlineDoc(sb, funcAndArgs))
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

        public bool GenerateFunctionDoc { get; set; }

        //public string RoxygenDocPostamble { get; set; }

        //public string RoxygenExportTag { get; set; }
        public string MatlabInputParameterTag { get; set; }
        public string MatlabCommentMarker { get; set; }
        //public bool RoxyExportFunctions { get; set; }

        public void AddInlineDoccoLine(StringBuilder sb, string lineTxt = "")
        {
            AddLine(sb, MatlabCommentMarker + lineTxt);
        }

        public bool CreateWrapFuncInlineDoc(StringBuilder sb, FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            //Aim for:
            
            //% Create a SWIFT catchment with a specified hydrologic model. 
            //% Usage:
            //% f = CREATECATCHMENT(nodeIds, nodeNames, linkIds, linkNames, linkFromNode, linkToNode, runoffModelName, areasKm2)
            //% This function is intended mostly for testing, not for usual modelling code.
            //% INPUT nodeIds[1xN string] Node unique identifiers
            //% INPUT nodeNames[1xN string] Node display names
            //% INPUT linkIds[1xN string] Links unique identifiers
            //% INPUT linkNames[1xN string] Links display names
            //% INPUT linkFromNode[1xN string] Identifier of the links' upstream node
            //% INPUT linkToNode[1xN string] Identifier of the links' downstream node
            //% INPUT runoffModelName[string] A valid, known SWIFT model name(e.g. 'GR5H')
            //% INPUT areasKm2[1xN double] The areas in square kilometres
            //% OUTPUT[libpointer] A SWIFT simulation object(i.e.a model runner)

            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            string funcName = funcDecl.VarName + FunctionNamePostfix;
            AddInlineDoccoLine(sb, " " + funcName);
            AddInlineDoccoLine(sb);
            AddInlineDoccoLine(sb, " " + funcName + " Wrapper function for " + funcDecl.VarName);
            AddInlineDoccoLine(sb);
            if (paramDocs)
            {
                var funcArgs = GetFuncArguments(funcAndArgs);
                for (int i = 0; i < funcArgs.Length; i++)
                {
                    var v = GetTypeAndName(funcArgs[i]);
                    AddInlineDoccoLine(sb, " " + MatlabInputParameterTag + " " + v.VarName + " R type equivalent for C++ type " + v.TypeName);
                }
            }
            //if (RoxyExportFunctions)
            //    AddInlineDoccoLine(sb, " " + RoxygenExportTag);
            return true;
        }

        public string ApiSignatureToBasicRoxygenString(FuncAndArgs funcAndArgs)
        {
            StringBuilder sb = new StringBuilder();
            CreateWrapFuncInlineDoc(sb, funcAndArgs, false);
            return sb.ToString();
        }

        public string ApiSignatureToRoxygenString(FuncAndArgs funcAndArgs, bool paramDocs = true)
        {
            StringBuilder sb = new StringBuilder();
            CreateWrapFuncInlineDoc(sb, funcAndArgs, paramDocs);
            return sb.ToString();
        }

        private bool createWrapFuncSignature(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            var funcDecl = GetTypeAndName(funcAndArgs.Function);
            // SWIFT_API OBJECTIVE_EVALUATOR_WILA_PTR CreateSingleObservationObjectiveEvaluatorWila(MODEL_SIMULATION_PTR simulation, const char* obsVarId, double* observations, TS_GEOMETRY_PTR obsGeom, const char* statisticId);
            // function f = CreateSingleObservationObjectiveEvaluatorWila_m(simulation, obsVarId, observations, obsGeom, statisticId)
            string funcDef = "function f = " + funcDecl.VarName + FunctionNamePostfix; // function f = CreateSingleObservationObjectiveEvaluatorWila_m
            sb.Append(funcDef);
            bool r = AddFunctionArgs(sb, funcAndArgs, ApiArgToMatlabfunctionArgument);
            sb.Append(' ');
            return r;
        }

        private void ApiArgToMatlabfunctionArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName);
        }

        protected override void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal)
        {
            if (returnsVal)
            {
                // E.G. 
                // f = createxptr(res, 'MODEL_SIMULATION_PTR')
                AddBodyLine(sb, "f = " + CreateXptrObjRefFunction + @"(" + ReturnedValueVarname + ", '" + funcDef.TypeName + "')");
            }
        }
    }
}
