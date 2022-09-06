using System;
using System.Collections.Generic;
using System.Text;

namespace ApiWrapperGenerator
{
    public class MatlabApiWrapperGenerator : BaseApiConverter
    {
        public MatlabApiWrapperGenerator()
        {
            ApiCallOpenParenthesis = false; // a kludge switch to cater for matlab's calllib
            FunctionOutputName = "f";
            FunctionNamePostfix = "";
            NativeLibraryNameNoext = "mylibname";

            UniformIndentationCount = 0;
            Indentation = "    ";

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

            SetTransientArgConversion(
                "char**",
                "_charpp",
            // pNodeIds = libpointer('stringPtrPtr', nodeIds);
            "C_ARGNAME = libpointer('stringPtrPtr', RCPP_ARGNAME);",
            // clear pNodeIds;
                "clear(C_ARGNAME);");

            // // All structs are marshalled in a similar manner:
            // string createPtr =
            //     // IntPtr geom_struct = InteropHelper.StructureToPtr(geom);
            //     // IntPtr C_ARGNAME = InteropHelper.StructureToPtr(RCPP_ARGNAME);
            //     "IntPtr C_ARGNAME = InteropHelper.StructureToPtr(RCPP_ARGNAME);";
            // string freePtr =
            //     // InteropHelper.FreeNativeStruct(geom_struct, ref geom, true);
            //     // InteropHelper.FreeNativeStruct(C_ARGNAME, ref RCPP_ARGNAME, true);
            //     "InteropHelper.FreeNativeStruct(C_ARGNAME, ref RCPP_ARGNAME, true);";

            //SetTypeMap("TS_GEOMETRY_PTR", "ref MarshaledTimeSeriesGeometry");
            SetTransientArgConversion(
                "TS_GEOMETRY_PTR",
                "_struct",
                // e.g. matlab\native\estimateDualPassParameters.m
                //tsGeo = createTsGeometry(dts, timeStepInSeconds, lenData);
                //pTsGeo = libstruct('MarshaledTsGeometry', tsGeo);
                "C_ARGNAME = createTsGeometry(RCPP_ARGNAME);",
                ""); // No cleanup? really?

            SetTypeMap("DATE_TIME_INFO_PTR", "ref MarshaledDateTime");
            SetTransientArgConversion(
                "DATE_TIME_INFO_PTR",
                "_struct",
                // estimationEndDt = createDateTimeFrom(estimationEnd);    
                "C_ARGNAME = createDateTimeFrom(RCPP_ARGNAME);",
                ""); // No cleanup? really?

            string createArrayStructPtr =
                "IntPtr C_ARGNAME = InteropHelper.ArrayOfStructureToPtr(RCPP_ARGNAME);";
            string freeArrayStructPtr =
                "InteropHelper.FreeNativeArrayOfStruct(C_ARGNAME, ref RCPP_ARGNAME, false);";

            //#define NODE_INFO_PTR  NodeInfoTxt*
            SetTransientArgConversion(
                "NODE_INFO_PTR",
                "_struct", createArrayStructPtr, freeArrayStructPtr);

            //#define LINK_INFO_PTR  LinkInfoTxt*
            SetTransientArgConversion(
                "LINK_INFO_PTR",
                "_struct", createArrayStructPtr, freeArrayStructPtr);

            SetTransientArgConversion("double**", "_doublepp",
                "IntPtr C_ARGNAME = InteropHelper.BiArrayDoubleToNative(RCPP_ARGNAME);",
                "InteropHelper.FreeBiArrayDouble(C_ARGNAME, RCPP_ARGNAME.Length);");

            SetTransientArgConversion("double*", "_doublep",
                // pAreasKm2 = libpointer('doublePtr', areasKm2);
                "C_ARGNAME = libpointer('doublePtr', RCPP_ARGNAME);",
                "clear(C_ARGNAME);");

            //SWIFT_API COMPOSITE_PARAMETERIZER_PTR AggregateParameterizers(const char* strategy, ARRAY_OF_PARAMETERIZERS_PTR parameterizers, int numParameterizers);
            //         INativeParameterizer AggregateParameterizers_cs(string strategy, INativeParameterizer[] parameterizers, int numParameterizers)
            SetTransientArgConversion(
                "ARRAY_OF_PARAMETERIZERS_PTR",
                "_array_ptr",
                //IntPtr parameterizers_array_ptr = InteropHelper.CreateNativeArray(Array.ConvertAll(parameterizers, p => p.GetHandle()));
                "IntPtr C_ARGNAME = InteropHelper.CreateNativeArray(Array.ConvertAll(RCPP_ARGNAME, p => p.GetHandle()));",
                "InteropHelper.DeleteNativeArray(C_ARGNAME);");

            SetTransientArgConversion(".*_PTR", "",
                "C_ARGNAME = " + GetXptrFromObjRefFunction + @"(RCPP_ARGNAME)" + StatementSep, //    x_ptr = getSwiftXptr(x);
                ""); // no cleanup

            // typenames followed by at least one *
            SetTransientArgConversion(".*\\*+", "",
                "C_ARGNAME = " + GetXptrFromObjRefFunction + @"(RCPP_ARGNAME)" + StatementSep, //    x_ptr = getSwiftXptr(x);
                ""); // no cleanup

        }

        /// <summary> Gets or sets the name of the output of the matlab functon being generated</summary>
        public string FunctionOutputName { get; set; }

        public CustomFunctionWrapperImpl ReturnsCharPtrPtrWrapper()
        {
            CustomFunctionWrapperImpl cw = new CustomFunctionWrapperImpl()
            {
                IsMatchFunc = StringHelper.ReturnsCharPP,
                StatementSep = this.StatementSep,
                ApiArgToWrappingLang = ApiArgToMatlabfunctionArgument,
                ApiCallArgument = this.ApiCallArgument,
                TransientArgsCreation = this.TransientArgsCreation,
                FunctionNamePostfix = this.FunctionNamePostfix,
                CalledFunctionNamePostfix = this.ApiCallPostfix,
                ApiSignatureToDocString = this.ApiSignatureToBasicRoxygenString,
                Template = @"
function f = %WRAPFUNCTION%(%WRAPARGS%)
%WRAPFUNCTIONDOCSTRING%
    pSize = libpointer('int32Ptr', 0);

    result = calllib('" + NativeLibraryNameNoext + @"', '%FUNCTION%', %ARGS%, pSize);

    len = pSize.Value;

    resCell = cell(1,len);
    for i=1:len,
        nP = result + (i - 1);
        resCell(i) = nP.Value;
    end

    f = resCell;

    clear pSize;
    clear result;
end
"
            };

            return cw;
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

        protected override void CreateApiFunctionCallFunction(StringBuilder sb, TypeAndName funcDef)
        {
            string matlabCallsLib = "calllib('" + NativeLibraryNameNoext + "', '" + funcDef.VarName + "', ";
            sb.Append(matlabCallsLib);
        }

        private void ApiCallArgument(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(typeAndName.VarName);
        }

        public bool GenerateFunctionDoc { get; set; }

        //public string RoxygenDocPostamble { get; set; }

        //public string RoxygenExportTag { get; set; }
        public string MatlabInputParameterTag { get; set; }
        public string NativeLibraryNameNoext { get; set; }
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
            bool returnsVal = FunctionReturnsValue(funcDecl);

            string funcDef = string.Format("function {0} = ", returnsVal ? FunctionOutputName : "[]");

            // SWIFT_API OBJECTIVE_EVALUATOR_WILA_PTR CreateSingleObservationObjectiveEvaluatorWila(MODEL_SIMULATION_PTR simulation, const char* obsVarId, double* observations, TS_GEOMETRY_PTR obsGeom, const char* statisticId);
            // function f = CreateSingleObservationObjectiveEvaluatorWila_m(simulation, obsVarId, observations, obsGeom, statisticId)
            funcDef += funcDecl.VarName + FunctionNamePostfix; // function f = CreateSingleObservationObjectiveEvaluatorWila_m
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
                if (IsPointer(funcDef.TypeName))
                    AddBodyLine(sb, FunctionOutputName+ " = " + CreateXptrObjRefFunction + @"(" + ReturnedValueVarname + ", '" + funcDef.TypeName + "')");
                else
                    AddBodyLine(sb, FunctionOutputName + " = " + ReturnedValueVarname);
            }
        }
    }
}
