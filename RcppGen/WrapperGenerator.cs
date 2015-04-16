using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rcpp.CodeGen
{
    public class WrapperGenerator
    {
        private Dictionary<string, string> typeMap;

        // CharacterVector nodeIds
        // char** nodeIdsChar = createAnsiStringArray(nodeIds);
        // freeAnsiStringArray(nodeIdsChar, nodeIds.length());
        private Dictionary<string, ArgConvertion> fromRcppArgConverter;

        private List<Tuple<Func<string, bool>, Func<string, string>>> customWrappers;

        private class ArgConvertion
        {
            string VariablePostfix;
            string SetupTemplate;
            string CleanupTemplate;

            public ArgConvertion(string variablePostfix, string setupTemplate, string cleanupTemplate)
            {
                VariablePostfix = variablePostfix;
                SetupTemplate = setupTemplate;
                CleanupTemplate = cleanupTemplate;
            }

            internal string GetSetup(string vname)
            {
                return ReplaveVariables(vname, SetupTemplate);
            }

            private string ReplaveVariables(string vname, string template)
            {
                return template.Replace("C_ARGNAME", GetTransientVarname(vname)).Replace("RCPP_ARGNAME", vname);
            }

            internal string GetTransientVarname(string vname)
            {
                return vname + VariablePostfix;
            }

            internal string GetCleanup(string vname)
            {
                return ReplaveVariables(vname, CleanupTemplate);
            }
        }

        public Dictionary<string, string> TypeMap
        {
            get { return typeMap; }
            set { typeMap = value; }
        }


        public WrapperGenerator()
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
            typeMap["bool"] = "LogicalVector";
            typeMap["const char"] = "CharacterVector";
            typeMap["const int"] = "IntegerVector";
            typeMap["const double"] = "NumericVector";
            typeMap["const char*"] = "CharacterVector";
            typeMap["const int*"] = "IntegerVector";
            typeMap["const double*"] = "NumericVector";

            fromRcppArgConverter = new Dictionary<string, ArgConvertion>();
            fromRcppArgConverter["char**"] = new ArgConvertion("_charpp", "char** C_ARGNAME = createAnsiStringArray(RCPP_ARGNAME);", "freeAnsiStringArray(C_ARGNAME, RCPP_ARGNAME.length());");

            ContainsAny = new string[] { "SWIFT_API" };
            ToRemove = new string[] { "SWIFT_API" };
            ContainsNone = new string[] { "#define" };
            NotStartsWith = new string[] { "//" };
            PointersEndsWithAny = new string[] { "*", "_PTR" };
            OpaquePointerClassName = "OpaquePointer";
            PrependOutputFile = "// This file was GENERATED\n//Do NOT modify it manually, as you are very likely to lose work\n\n" +
                @"

#ifndef STRDUP
#ifdef _WIN32
#define STRDUP _strdup
#else
#define STRDUP strdup
#endif
#endif

#ifndef Rcpp_hpp
#include <Rcpp.h>
#endif

using namespace Rcpp;

char** createAnsiStringArray(CharacterVector charVec)
{
	char** res = new char*[charVec.length()];
	for (size_t i = 0; i < charVec.length(); i++)
		res[i] = STRDUP(as<std::string>(charVec[i]).c_str());
	return res;
}

void freeAnsiStringArray(char ** values, int arrayLength)
{
	for (int i = 0; i < arrayLength; i++)
		delete[] values[i];
	delete[] values;
}

CharacterVector toVectorCleanup(char** names, int size)
{
	CharacterVector v(size);
	for (size_t i = 0; i < size; i++)
		v[i] = std::string(names[i]);
	DeleteAnsiStringArray(names, size);
	return v;
}


";

            customWrappers = new List<Tuple<Func<string, bool>, Func<string, string>>>();
            customWrappers.Add(Tuple.Create(
                (Func<string, bool>)ReturnsCharPP, 
                (Func<string, string>)WrapCharPPRetVal));
        }

        public bool DeclarationOnly { get; set; }

        public bool OpaquePointers { get; set; }

        public string OpaquePointerClassName { get; set; }

        public string NewLineString { get; set; }

        public bool AddRcppExport { get; set; }

        public string FunctionNamePostfix { get; set; }

        public string PrependOutputFile { get; set; }

        public string[] PointersEndsWithAny { get; set; }

        public string[] NotStartsWith { get; set; }

        public string[] ToRemove { get; set; }

        public string[] ContainsAny { get; set; }

        public string[] ContainsNone { get; set; }

        public void CreateWrapperHeader(string inputFile, string outputFile)
        {
            string input = File.ReadAllText(inputFile);
            string output = CHeaderToRcpp(input);
            File.WriteAllText(outputFile, output);
        }

        public void SetTypeMap(string cType, string rcppType)
        {
            typeMap[cType] = rcppType;
        }

        public string CHeaderToRcpp(string input)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            StringBuilder sb = new StringBuilder();
            sb.Append(PrependOutputFile);
            using (var tr = new StringReader(input))
            {
                string line = "";
                while (line != null)
                {
                    line = line.Trim();
                    if (IsMatch(line))
                    {
                        sb.Append(LineCHeaderToRcpp(line));
                        sb.Append(NewLineString);
                    }
                    line = tr.ReadLine();
                }
                return sb.ToString();
            }
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
            //     return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->ptr)));
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
            string s = prepareInLine(line);
            // At this point we'd have:
            //ModelRunner* CreateNewFromNetworkInfo(NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks);
            // or
            //SWIFT_API MODEL_SIMULATION_PTR CreateNewFromNetworkInfo(NODE_INFO_PTR nodes, int numNodes, LINK_INFO_PTR links, int numLinks);
            s = s.Replace(");", "");
            string[] funcAndArgs = s.Split(new[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            return funcAndArgs;
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
            // 	return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->ptr)));
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
                sb.Append("    return " + RcppWrap(funcDef[0], "res") + ";");
            }
            return true;
        }

        private void findTransientVariables(string functionArguments, out Dictionary<string, string> transientArgs, out string[] transientArgsSetup, out string[] transientArgsCleanup)
        {
            transientArgs = new Dictionary<string, string>();
            List<string> setup = new List<string>(), cleanup = new List<string>();
            string[] args = splitOnComma(functionArguments);
            for (int i = 0; i < args.Length - 1; i++)
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
                string arg;
                string functionArguments = funcAndArgs[1];
                string[] args = splitOnComma(functionArguments);
                for (int i = 0; i < args.Length - 1; i++)
                {
                    arg = args[i];
                    if (!addArgument(sb, argFunc, transientArgs, arg)) return false;
                    sb.Append(", ");
                }
                arg = args[args.Length - 1];
                if (!addArgument(sb, argFunc, transientArgs, arg)) return false;
            }
            sb.Append(")");
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
            return CppToRTypes(typename) + "(" + varname + ")";
        }

        private void RcppToApiType(StringBuilder sb, string typename, string varname)
        {

            //void SetErrorCorrectionModel_R(XPtr<OpaquePointer> src, CharacterVector newModelId, CharacterVector elementId, IntegerVector length, IntegerVector seed)
            //{
            //    SetErrorCorrectionModel(src->ptr, as<char*>(newModelId), as<char*>(elementId), as<int>(length), as<int>(seed));
            //}
            if (isKnownType(typename))
                sb.Append(AddAs(typename, varname));
            else if (isPointer(typename))
            {
                if (typename.EndsWith("**") || typename.EndsWith("PTR*"))
                    sb.Append("(void**)");
                sb.Append(varname + "->ptr"); // src->ptr
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
CharacterVector %WRAPFUNCTION%(XPtr<OpaquePointer> modelInstance)
{
	int size; 
	char** names = %FUNCTION%(modelInstance->ptr, &size);
	return toVectorCleanup(names, size);
}
";
            return template.Replace("%WRAPFUNCTION%", wrapFuncName).Replace("%FUNCTION%", funcName);
        }
    }
}
