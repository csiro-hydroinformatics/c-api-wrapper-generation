using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rcpp.CodeGen
{

    public interface IApiConverter
    {
        string ConvertLine(string line);
        string GetPreamble();
    }

    public class WrapperGenerator
    {
        public WrapperGenerator(IApiConverter converter)
        {
            this.converter = converter;
            this.filter = new HeaderFilter();
        }
        public WrapperGenerator(IApiConverter converter, HeaderFilter filter)
        {
            this.converter = converter;
            this.filter = filter;
        }

        IApiConverter converter;
        HeaderFilter filter;

        public void CreateWrapperHeader(string inputFile, string outputFile)
        {
            string[] lines = filter.Filter(inputFile);
            StringBuilder sb = new StringBuilder();
            sb.Append(converter.GetPreamble());
            string[] outputlines = Convert(lines);
            for (int i = 0; i < outputlines.Length; i++)
            {
                sb.Append(outputlines[i]);
            }
            string output = sb.ToString();
            File.WriteAllText(outputFile, output);
        }

        public string[] Convert(string[] lines)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            List<string> converted = new List<string>();

            //sb.Append(PrependOutputFile);
            foreach(string lineRaw in lines)
            {
                string line = lineRaw.Trim();
                string convertedLine = converter.ConvertLine(line);
                converted.Add(convertedLine);
            }
            return converted.ToArray();
        }
    }

    public class HeaderFilter
    {
        public HeaderFilter()
        {
            ContainsAny = new string[] { "SWIFT_API" };
            ToRemove = new string[] { "SWIFT_API" };
            ContainsNone = new string[] { "#define" };
            NotStartsWith = new string[] { "//" };
        }

        public string[] Filter(string inputFile)
        {
            string input = File.ReadAllText(inputFile);
            return FindMatchingLines(input);
        }

        public string[] FindMatchingLines(string input)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            List<string> output = new List<string>();
            using (var tr = new StringReader(input))
            {
                string line = "";
                while (line != null)
                {
                    line = line.Trim();
                    if (IsMatch(line))
                    {
                        line = prepareInLine(line);
                        output.Add(line);
                    }
                    line = tr.ReadLine();
                }
            }
            return output.ToArray();
        }

        private string prepareInLine(string line)
        {
            string s = line.Replace("\t", " ");
            s = s.Trim();
            s = removeToRemove(s);
            s = s.Trim();
            s = preprocessPointers(s);
            return s;
        }

        private static string preprocessPointers(string s)
        {
            // Make all pointers types without blanks
            var rexpPtr = new Regex(" *\\*");
            s = rexpPtr.Replace(s, "*");
            return s;
        }

        private string removeToRemove(string s)
        {
            foreach (var r in ToRemove)
                s = s.Replace(r, "");
            return s;
        }
        
        public bool IsMatch(string line)
        {
            line = line.Trim();
            if (StartsWithExcluded(line)) return false;
            bool match = false;
            if (ContainsAny.Length > 0)
            {
                foreach (string p in ContainsAny)
                    match = match || line.Contains(p);
                if (!match) return false;
            }
            match = true;
            foreach (string p in ContainsNone)
                if (line.Contains(p)) return false;

            return match;
        }

        private bool StartsWithExcluded(string line)
        {
            foreach (string p in NotStartsWith)
                if (line.StartsWith(p)) return true;
            return false;
        }

        public string[] NotStartsWith { get; set; }

        public string[] ToRemove { get; set; }

        public string[] ContainsAny { get; set; }

        public string[] ContainsNone { get; set; }

    }

    public class ArgConversion
    {
        string VariablePostfix;
        string SetupTemplate;
        string CleanupTemplate;

        public ArgConversion(string variablePostfix, string setupTemplate, string cleanupTemplate)
        {
            VariablePostfix = variablePostfix;
            SetupTemplate = setupTemplate;
            CleanupTemplate = cleanupTemplate;
        }

        public string GetSetup(string vname)
        {
            return ReplaceVariables(vname, SetupTemplate);
        }

        public string ReplaceVariables(string vname, string template)
        {
            return template.Replace("C_ARGNAME", GetTransientVarname(vname)).Replace("RCPP_ARGNAME", vname);
        }

        public string GetTransientVarname(string vname)
        {
            return vname + VariablePostfix;
        }

        public string GetCleanup(string vname)
        {
            return ReplaceVariables(vname, CleanupTemplate);
        }
    }


    public class RXptrWrapperGenerator : IApiConverter
    {
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

        CreateObjectiveCalculator_R_wrap <- function(modelInstance, obsVarId, observations, arrayLength, start, statisticId) {
            modelInstance_xptr <- getSwiftXptr(modelInstance)
            xptr <- CreateObjectiveCalculator_R(modelInstance_xptr, obsVarId, observations, arrayLength, start, statisticId)
            return(mkSwiftObjRef(xptr))
        }

}

*/

        public string ConvertLine(string line)
        {
            throw new NotImplementedException();
        }

        public string GetPreamble()
        {
            throw new NotImplementedException();
        }
    }

    public class RcppGlueWrapperGenerator : IApiConverter
    {
        private Dictionary<string, string> typeMap;

        // CharacterVector nodeIds
        // char** nodeIdsChar = createAnsiStringArray(nodeIds);
        // freeAnsiStringArray(nodeIdsChar, nodeIds.length());
        private Dictionary<string, ArgConversion> fromRcppArgConverter;

        public void SetRcppArgConverter(string cArgType, string variablePostfix, string setupTemplate, string cleanupTemplate)
        {
            fromRcppArgConverter[cArgType] = new ArgConversion(variablePostfix, setupTemplate, cleanupTemplate);
        }

        private List<Tuple<Func<string, bool>, Func<string, string>>> customWrappers;

        public Dictionary<string, string> TypeMap
        {
            get { return typeMap; }
            set { typeMap = value; }
        }

        public RcppGlueWrapperGenerator()
        {
            FunctionNamePostfix = "_R";
            OpaquePointers = false;
            DeclarationOnly = false;
            AddRcppExport = true;
            NewLineString = "\n";

            typeMap = new Dictionary<string, string>();
            typeMap["void"] = "void";
            typeMap["int"] = "IntegerVector";
            typeMap["int*"] = "IntegerVector";
            typeMap["char**"] = "CharacterVector";
            typeMap["char*"] = "CharacterVector";
            typeMap["char"] = "CharacterVector";
            typeMap["double"] = "NumericVector";
            typeMap["double*"] = "NumericVector";
            typeMap["double**"] = "NumericMatrix";
            typeMap["bool"] = "LogicalVector";
            typeMap["const char"] = "CharacterVector";
            typeMap["const int"] = "IntegerVector";
            typeMap["const double"] = "NumericVector";
            typeMap["const char*"] = "CharacterVector";
            typeMap["const int*"] = "IntegerVector";
            typeMap["const double*"] = "NumericVector";

            fromRcppArgConverter = new Dictionary<string, ArgConversion>();

            PointersEndsWithAny = new string[] { "*", "_PTR" };
            OpaquePointerClassName = "OpaquePointer";
            PrependOutputFile = "// This file was GENERATED\n//Do NOT modify it manually, as you are very likely to lose work\n\n";

            customWrappers = new List<Tuple<Func<string, bool>, Func<string, string>>>();
            customWrappers.Add(Tuple.Create(
                (Func<string, bool>)ReturnsCharPP, 
                (Func<string, string>)WrapCharPPRetVal));
        }

        public string GetPreamble()
        {
            return PrependOutputFile;
        }

        public bool DeclarationOnly { get; set; }

        public bool OpaquePointers { get; set; }

        public string OpaquePointerClassName { get; set; }

        public string NewLineString { get; set; }

        public bool AddRcppExport { get; set; }

        public string FunctionNamePostfix { get; set; }

        public string PrependOutputFile { get; set; }

        public string[] PointersEndsWithAny { get; set; }

        public void SetTypeMap(string cType, string rcppType)
        {
            typeMap[cType] = rcppType;
        }

        public string ConvertLine(string line)
        {
            string convertedLine = string.Empty;
            convertedLine += LineCHeaderToRcpp(line);
            convertedLine += NewLineString;
            return convertedLine;
        }

        private string GetReturnedType(string funDef)
        {
            string[] typeAndName = GetFunctionTypeAndName(funDef);
            return typeAndName[0];
        }

        private string[] GetFunctionTypeAndName(string funDef)
        {
            string[] funcAndArgs = GetFuncDeclAndArgs(funDef);
            return GetVariableDeclaration(funcAndArgs[0]);
        }

        private string[] GetFunctionArguments(string funDef)
        {
            string[] funcAndArgs = GetFuncDeclAndArgs(funDef);
            return splitOnComma(funcAndArgs[1]);
        }

        private string GetFuncName(string funDef)
        {
            string[] typeAndName = GetFunctionTypeAndName(funDef);
            return typeAndName[1];
        }

        public string LineCHeaderToRcpp(string line)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            // And as an output we want for instance (if using opaque pointers).
            // // [[Rcpp::export]]
            // XPtr<OpaquePointer> RcppCloneModel(XPtr<OpaquePointer> src)
            // {
            //     return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->Get())));
            // }

            foreach (var c in customWrappers)
            {
                if (c.Item1.Invoke(line))
                    return c.Item2.Invoke(line);
            }

            string[] funcAndArgs = GetFuncDeclAndArgs(line);
            if (funcAndArgs.Length > 2) return line; // bail out - just not sure what is going on.
            var sb = new StringBuilder();
            if (!createWrapFuncSignature(sb, funcAndArgs)) return line;
            if (DeclarationOnly)
            {
                sb.Append(";");
                return sb.ToString();
            }
            else
            {
                sb.Append(NewLineString); sb.Append("{"); sb.Append(NewLineString);
                if (!createWrapFuncBody(sb, funcAndArgs)) return line;
                sb.Append(NewLineString); sb.Append("}"); sb.Append(NewLineString);
                return sb.ToString();
            }
        }

        private string[] GetFuncDeclAndArgs(string line)
        {
            string s = line;
            // At this point we'd have:
            //ModelRunner* CreateNewFromNetworkInfo(NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks);
            // or
            //SWIFT_API MODEL_SIMULATION_PTR CreateNewFromNetworkInfo(NODE_INFO_PTR nodes, int numNodes, LINK_INFO_PTR links, int numLinks);
            s = s.Replace(");", "");
            string[] funcAndArgs = s.Split(new[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            return funcAndArgs;
        }

        private bool createWrapFuncSignature(StringBuilder sb, string[] funcAndArgs)
        {
            if (AddRcppExport)
                sb.Append("// [[Rcpp::export]]" + NewLineString);
            Action<StringBuilder, string[]> argFunc = ApiArgToRcpp;
            string funcDef = funcAndArgs[0] + FunctionNamePostfix;
            if (!ParseTypeAndName(sb, funcDef, argFunc)) return false;
            return AddFunctionArgs(sb, funcAndArgs, argFunc);
        }

        private bool createWrapFuncBody(StringBuilder sb, string[] funcAndArgs)
        {
            Action<StringBuilder, string[]> argFunc = ApiCallArgument;
            // We need to cater for cases where we need to create a transient variable then clean it, e.g.
            // char** c = transform((CharacterVector)cvec);
            // apiCall(c)
            // cleanup(c)

            Dictionary<string, string> transientArgs = null;
            string[] transientArgsSetup = null;
            string[] transientArgsCleanup = null;
            if (funcAndArgs.Length > 1)
            {
                string functionArguments = funcAndArgs[1];
                findTransientVariables(functionArguments, out transientArgs, out transientArgsSetup, out transientArgsCleanup);
                foreach (var item in transientArgsSetup)
                    sb.AppendLine("    " + item); // e.g. char** linkIdsChar = createAnsiStringArray(linkIds);
            }

            string[] funcDef = GetTypeAndName(funcAndArgs[0]);
            bool returnsVal = (funcDef[0].Trim() != "void");
            // 	return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->Get())));
            sb.Append("    ");
            if (returnsVal)
                sb.Append("auto res = ");
            sb.Append(funcDef[1]);
            if (!AddFunctionArgs(sb, funcAndArgs, argFunc, transientArgs)) return false;
            sb.Append(";");
            sb.Append(NewLineString);
            if (funcAndArgs.Length > 1)
                foreach (var item in transientArgsCleanup)
                    sb.AppendLine("    " + item); // e.g. freeAnsiStringArray(nodeIdsChar, nodeIds.length());

            if (returnsVal)
            {
                sb.Append("    auto x = " + RcppWrap(funcDef[0], "res") + ";" + NewLineString);
                if(funcDef[0] == "char*")
                    sb.Append("    DeleteAnsiString(res);" + NewLineString);
                sb.Append("    return x;");
            }
            return true;
        }

        private void findTransientVariables(string functionArguments, out Dictionary<string, string> transientArgs, out string[] transientArgsSetup, out string[] transientArgsCleanup)
        {
            transientArgs = new Dictionary<string, string>();
            List<string> setup = new List<string>(), cleanup = new List<string>();
            string[] args = splitOnComma(functionArguments);
            for (int i = 0; i < args.Length; i++)
            {
                var varDecl = GetVariableDeclaration(args[i]); // "const int*" "blah"
                addTransientVariable(varDecl, transientArgs, setup, cleanup);
            }
            transientArgsSetup = setup.ToArray();
            transientArgsCleanup = cleanup.ToArray();
        }

        private void addTransientVariable(string[] varDecl, Dictionary<string, string> transientArgs, List<string> setup, List<string> cleanup)
        {
            string tname = varDecl[0];
            string vname = varDecl[1];
            if (!fromRcppArgConverter.ContainsKey(tname))
                return;
            var confInfo = fromRcppArgConverter[tname];
            setup.Add(confInfo.GetSetup(vname));
            transientArgs.Add(vname, confInfo.GetTransientVarname(vname));
            cleanup.Add(confInfo.GetCleanup(vname));
        }

        private bool AddFunctionArgs(StringBuilder sb, string[] funcAndArgs, Action<StringBuilder, string[]> argFunc, Dictionary<string, string> transientArgs = null)
        {
            sb.Append("(");
            if (funcAndArgs.Length > 1)
            {
                string functionArguments = funcAndArgs[1];
                string[] args = splitOnComma(functionArguments);
                int start = 0, end = args.Length - 1;
                if (!appendArgs(sb, argFunc, transientArgs, args, start, end)) return false;
                if(end > start)
                    sb.Append(", ");
                string arg = args[args.Length - 1];
                if (!addArgument(sb, argFunc, transientArgs, arg)) return false;
            }
            sb.Append(")");
            return true;
        }

        private bool appendArgs(StringBuilder sb, Action<StringBuilder, string[]> argFunc, Dictionary<string, string> transientArgs, string[] args, int start, int end)
        {
            string arg;
            for (int i = start; i < end; i++)
            {
                arg = args[i];
                if (!addArgument(sb, argFunc, transientArgs, arg)) return false;
                if (i < (end - 1))
                    sb.Append(", ");
            }
            return true;
        }

        private bool addArgument(StringBuilder sb, Action<StringBuilder, string[]> argFunc, Dictionary<string, string> transientArgs, string arg)
        {
            var typeAndName = GetVariableDeclaration(arg);
            if (typeAndName.Length != 2) return false;
            string vname = typeAndName[1];
            if (transientArgs != null && transientArgs.ContainsKey(vname))
            {
                sb.Append(transientArgs[vname]);
                return true;
            }
            return ParseTypeAndName(sb, arg, argFunc);
        }

        private static string[] splitOnComma(string functionArguments)
        {
            string[] args = functionArguments.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); //NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks
            return args;
        }

        private bool ParseTypeAndName(StringBuilder sb, string argString, Action<StringBuilder, string[]> fun = null)
        {
            // argString could be something like:
            // double x
            // const char* s
            // ModelRunner * s
            var typeAndName = GetVariableDeclaration(argString);

            if (typeAndName.Length != 2) return false;
            fun(sb, typeAndName);
            return true;
        }

        private string[] GetVariableDeclaration(string argString)
        {
            var typeAndName = argString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);//"ModelRunner*" "CreateNewFromNetworkInfo"
            // cater for things like const char* s:
            if (typeAndName.Length > 2)
                typeAndName = new[]{ 
                    Concat(typeAndName, 0, typeAndName.Length-1),
                    typeAndName[typeAndName.Length-1]};
            return typeAndName;
        }

        private string[] GetTypeAndName(string argString)
        {
            // argString could be something like:
            // double x
            // const char* s
            // ModelRunner * s
            var typeAndName = argString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);//"ModelRunner*" "CreateNewFromNetworkInfo"
            // cater for things like const char* s:
            if (typeAndName.Length > 2)
                typeAndName = new[]{ 
                    Concat(typeAndName, 0, typeAndName.Length-1),
                    typeAndName[typeAndName.Length-1]};
            return typeAndName;
        }

        private void ApiArgToRcpp(StringBuilder sb, string[] typeAndName)
        {
            var rt = typeAndName[0];
            ApiTypeToRcpp(sb, rt);
            sb.Append(" ");
            sb.Append(typeAndName[1]);
        }

        private void ApiCallArgument(StringBuilder sb, string[] typeAndName)
        {
            RcppToApiType(sb, typeAndName[0], typeAndName[1]);
        }

        private string Concat(string[] elemts, int start, int count, string sep = " ")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count - 1; i++)
            {
                sb.Append(elemts[start + i]);
                sb.Append(sep);
            }
            sb.Append(elemts[start + count - 1]);
            return sb.ToString();
        }

        private void ApiTypeToRcpp(StringBuilder sb, string typename)
        {
            if (isKnownType(typename))
                sb.Append(CppToRTypes(typename));
            else if (isPointer(typename))
                sb.Append(createXPtr(typename)); // XPtr<ModelRunner>
            else
                sb.Append(CppToRTypes(typename));
        }

        private bool isPointer(string typename)
        {
            foreach (string p in PointersEndsWithAny)
                if (typename.EndsWith(p)) return true;
            return false;
        }

        private string RcppWrap(string typename, string varname)
        {
            if (isKnownType(typename))
                return WrapAsRcppVector(typename, varname);
            else if (isPointer(typename))
                return (createXPtr(typename, varname, true)); // XPtr<ModelRunner>(new OpaquePointer(varname))
            else
                return WrapAsRcppVector(typename, varname);
        }

        private bool isKnownType(string typename)
        {
            return typeMap.ContainsKey(typename);
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
            if (isKnownType(typename))
                sb.Append(AddAs(typename, varname));
            else if (isPointer(typename))
            {
                if (typename.EndsWith("**") || typename.EndsWith("PTR*"))
                    sb.Append("(void**)");
                sb.Append(varname + "->Get()"); // src->Get()
            }
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
            var s = rt.Trim();
            if (typeMap.ContainsKey(s)) return typeMap[s]; else return s;
        }



        // Below are more tricky ones, not yet fully fleshed out support.


        public bool ReturnsCharPP(string funDef)
        {
            return (GetReturnedType(funDef) == "char**");
        }

        public string WrapCharPPRetVal(string funDef)
        {
            string funcName = GetFuncName(funDef);
            string wrapFuncName = funcName + this.FunctionNamePostfix;
            var template = @"
// [[Rcpp::export]]
CharacterVector %WRAPFUNCTION%(%WRAPARGS%)
{
	int size; 
	char** names = %FUNCTION%(%ARGS% &size);
	return toVectorCleanup(names, size);
}
";
            return template
                .Replace("%WRAPARGS%", WrapArgsDecl(funDef, 0, 0))
                .Replace("%ARGS%", FuncCallArgs(funDef, 0, 0))
                .Replace("%WRAPFUNCTION%", wrapFuncName)
                .Replace("%FUNCTION%", funcName);
        }

        private string WrapArgsDecl(string funDef, int start, int offsetLength)
        {
            return CreateFunctionArgs(funDef, start, offsetLength, ApiArgToRcpp);
        }

        private string FuncCallArgs(string funDef, int start, int offsetLength)
        {
            return CreateFunctionArgs(funDef, start, offsetLength, ApiCallArgument, appendComma: true);
        }

        private string CreateFunctionArgs(string funDef, int start, int offsetLength, Action<StringBuilder, string[]> argFunc, bool appendComma=false)
        {
            StringBuilder sb = new StringBuilder();
            var args = GetFunctionArguments(funDef);
            int end = args.Length - 1 - offsetLength;
            appendArgs(sb, argFunc, null, args, 0, end);
            if (appendComma && (end > start)) sb.Append(", ");
            return sb.ToString();
        }
    }
}
