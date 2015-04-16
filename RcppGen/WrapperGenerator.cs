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

            ContainsAny = new string[] { "SWIFT_API" };
            ToRemove = new string[] { "SWIFT_API" };
            ContainsNone = new string[] { "#define" };
            OpaquePointerClassName = "OpaquePointer";
        }

        public bool DeclarationOnly { get; set; }

        public bool OpaquePointers { get; set; }

        public string OpaquePointerClassName { get; set; }

        public string NewLineString { get; set; }

        public bool AddRcppExport { get; set; }

        public string FunctionNamePostfix { get; set; }

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
            using (var tr = new StringReader(input))
            {
                string line = "";
                while (line != null)
                {
                    line = line.Trim();
                    if(IsMatch(line))
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
            if (line.StartsWith("//")) return false;
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

            string s = line.Replace("\t", " ");
            s = s.Trim();
            var sb = new StringBuilder();
            s = removeToRemove(s);
            s = s.Trim();
            s = preprocessPointers(s);
            // At this point we'd have:
            //ModelRunner* CreateNewFromNetworkInfo(NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks);
            // or
            //SWIFT_API MODEL_SIMULATION_PTR CreateNewFromNetworkInfo(NODE_INFO_PTR nodes, int numNodes, LINK_INFO_PTR links, int numLinks);
            s = s.Replace(");", "");
            string[] funcAndArgs = s.Split(new[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            if (funcAndArgs.Length > 2) return line; // bail out - just not sure what is going on.
            if (!createWrapFuncSignature(sb, funcAndArgs)) return line;
            if (DeclarationOnly)
            {
                sb.Append(";");
                return sb.ToString();
            }
            else
            {
                sb.Append(NewLineString); sb.Append("{"); sb.Append(NewLineString);
                sb.Append("    ");
                if (!createWrapFuncBody(sb, funcAndArgs)) return line;
                sb.Append(NewLineString); sb.Append("}"); sb.Append(NewLineString);
                return sb.ToString();
            }
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
            Action<StringBuilder,string[]> argFunc = ApiArgToRcpp;
            string funcDef = funcAndArgs[0] + FunctionNamePostfix;
            if (!ParseTypeAndName(sb, funcDef, argFunc)) return false;
            return AddFunctionArgs(sb, funcAndArgs, argFunc);
        }

        private bool createWrapFuncBody(StringBuilder sb, string[] funcAndArgs)
        {
            Action<StringBuilder, string[]> argFunc = ApiCallArgument;
            string[] funcDef = GetTypeAndName(funcAndArgs[0]);

            // 	return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->ptr)));
            if(funcDef[0].Trim() != "void")
                sb.Append("auto res = ");
            sb.Append(funcDef[1]);
            if (!AddFunctionArgs(sb, funcAndArgs, argFunc)) return false;
            sb.Append(";");
            if (funcDef[0].Trim() != "void")
            {
                sb.Append(NewLineString);
                sb.Append("    return " + RcppWrap(funcDef[0], "res") + ";");
            }
            return true;
        }


        private bool AddFunctionArgs(StringBuilder sb, string[] funcAndArgs, Action<StringBuilder, string[]> argFunc)
        {
            sb.Append("(");
            if (funcAndArgs.Length > 1)
            {
                string[] args = funcAndArgs[1].Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); //NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks
                for (int i = 0; i < args.Length - 1; i++)
                {
                    var arg = args[i];
                    if (!ParseTypeAndName(sb, arg, argFunc)) return false;
                    sb.Append(", ");
                }
                if (!ParseTypeAndName(sb, args[args.Length - 1], argFunc)) return false;
            }
            sb.Append(")");
            return true;
        }

        private bool ParseTypeAndName(StringBuilder sb, string argString, Action<StringBuilder,string[]> fun=null)
        {
            // argString could be something like:
            // double x
            // const char* s
            // ModelRunner * s
            var typeAndName = argString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);//"ModelRunner*" "CreateNewFromNetworkInfo"
            // cater for things like const char* s:
            if(typeAndName.Length > 2)
                typeAndName = new[]{ 
                    Concat(typeAndName, 0, typeAndName.Length-1),
                    typeAndName[typeAndName.Length-1]};

            if (typeAndName.Length != 2) return false;
            fun(sb, typeAndName);
            return true;
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
            for (int i = 0; i < count-1; i++)
            {
                sb.Append(elemts[start + i]);
                sb.Append(sep);
            }
            sb.Append(elemts[start + count-1]);
            return sb.ToString();
        }

        private void ApiTypeToRcpp(StringBuilder sb, string typename)
        {
            if(isKnownType(typename))
                sb.Append(CppToRTypes(typename));
            else if (isPointer(typename))
                sb.Append(createXPtr(typename)); // XPtr<ModelRunner>
            else
                sb.Append(CppToRTypes(typename));
        }

        private bool isPointer(string typename)
        {
            return (typename.EndsWith("*") || typename.EndsWith("_PTR"));
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
            {
                if (typename.EndsWith("**") || typename.EndsWith("PTR*"))
                    sb.Append("(void**)");
                sb.Append(varname + "->ptr"); // src->ptr
            }
            else if (isPointer(typename))
                sb.Append(AddAs(typename, varname));
            else
                sb.Append(AddAs(typename, varname));
        }

        private string AddAs(string typename, string varname)
        {
            if (typename.EndsWith("char*"))
                return varname + "[0]";
            if (typename.EndsWith("double*"))
                return "&(" + varname + "[0])";
            return ("as<" + typename + ">(" + varname + ")");
        }

        private string createXPtr(string typePtr, string varname="", bool instance=false) // ModelRunner* becomes   XPtr<ModelRunner>
        {
            string res;
            if (OpaquePointers)
                res = "XPtr<" + OpaquePointerClassName +">";
            else
                res = "XPtr<" + typePtr.Replace("*", "") + ">";
            if (instance)
            {
                if (OpaquePointers)
                    res = res + "(new "+ OpaquePointerClassName +"("+varname+"))";
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

    }
}
