using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiWrapperGenerator
{
    public abstract class BaseApiConverter
    {
        protected BaseApiConverter()
        {
            NewLineString = StringHelper.NewLineString;
            FunctionBodyOpenDelimiter = NewLineString + "{" + NewLineString;
            FunctionBodyCloseDelimiter = NewLineString + "}" + NewLineString;
            StatementSep = ";";
            ApiCallPostfix = string.Empty;
            PointersEndsWithAny = new string[] { "*", "_PTR" };
            CallGetMethod = "->Get()";
            CastToOpaquePtrPtr = "(void**)";
            Indentation = "    ";
        }

        public string Indentation { get; set; }

        public string FunctionBodyOpenDelimiter { get; set; }

        public string FunctionBodyCloseDelimiter { get; set; }

        public string ApiCallPostfix { get; set; }

        public string StatementSep { get; set; }

        public string FunctionNamePostfix { get; set; }

        public string[] PointersEndsWithAny { get; set; }

        public string NewLineString { get; set; }

        public bool DeclarationOnly { get; set; }

        public string CallGetMethod { get; set; }

        public string CastToOpaquePtrPtr { get; set; }

        public string PrependOutputFile { get; set; }

        public string AssignmentSymbol { get; set; }

        public string ReturnedValueVarname { get; set; }

        protected List<ICustomFunctionWrapper> customWrappers =
            new List<ICustomFunctionWrapper>();

        private Dictionary<string, string> typeMap = new Dictionary<string, string>();

        public Dictionary<string, string> TypeMap
        {
            get { return typeMap; }
            set { typeMap = value; }
        }

        public string ConvertLine(string line)
        {
            string convertedLine = string.Empty;
            convertedLine += ConvertApiLine(line);
            convertedLine += NewLineString;
            return convertedLine;
        }

        public string ConvertApiLine(string line)
        {
            if (MatchesCustomWrapper(line))
                return ApplyCustomWrapper(line);

            var funcAndArgs = StringHelper.GetFuncDeclAndArgs(line);
            if (funcAndArgs.Unexpected)
                return line; // bail out - just not sure what is going on.
            return ConvertApiLineSpecific(line, funcAndArgs);
        }

        public abstract string ConvertApiLineSpecific(string line, FuncAndArgs funcAndArgs);

        protected string DefaultAnsiCToWrapperType(string rt)
        {
            var s = rt.Trim();
            if (TypeMap.ContainsKey(s)) return TypeMap[s]; else return s;
        }

        protected bool IsPointerPointer(string typename)
        {
            string t = typename.Trim();
            foreach (var ptrTermination in PointersEndsWithAny)
            {
                if (t.EndsWith(ptrTermination + "*"))
                    return true;
            }
            return false;
        }

        protected void ConvertPointerTypeToCapi(StringBuilder sb, string typename, string varname)
        {
            if (IsPointerPointer(typename))
                sb.Append(CastToOpaquePtrPtr);
            string callGetPtr = callgetPtrStatement(varname);
            sb.Append(callGetPtr);
        }


        private string callgetPtrStatement(string varname)
        {
            return varname + CallGetMethod;
            // src->Get()
        }

        public string GetPreamble()
        {
            return PrependOutputFile;
        }

        public void ClearCustomWrappers()
        {
            customWrappers.Clear();
        }

        public void AddCustomWrapper(CustomFunctionWrapperImpl cw)
        {
            customWrappers.Add(cw);
        }

        protected static bool AddFunctionArgs(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, string> transientArgs = null)
        {
            sb.Append("(");
            string[] args = GetFuncArguments(funcAndArgs);
            if (args.Length > 0)
            {
                int start = 0, end = args.Length - 1;
                if (!StringHelper.appendArgs(sb, argFunc, transientArgs, args, start, end)) return false;
                if (end > start)
                    sb.Append(", ");
                string arg = args[args.Length - 1];
                if (!StringHelper.addArgument(sb, argFunc, transientArgs, arg)) return false;
            }
            sb.Append(")");
            return true;
        }

        protected static string[] GetFuncArguments(FuncAndArgs funcAndArgs)
        {
            string functionArguments = funcAndArgs.Arguments;
            string[] args = StringHelper.SplitOnComma(functionArguments);
            return args;
        }

        public string ApplyCustomWrapper(string line)
        {
            foreach (var c in customWrappers)
            {
                if (c.IsMatch(line))
                    return c.CreateWrapper(line, DeclarationOnly);
            }
            return line;
        }

        public bool MatchesCustomWrapper(string line)
        {
            foreach (var c in customWrappers)
            {
                if (c.IsMatch(line))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a mapping from a C type to a corresponding type in the target language
        /// </summary>
        /// <param name="cType"></param>
        /// <param name="rcppType"></param>
        public void SetTypeMap(string cType, string rcppType)
        {
            typeMap[cType] = rcppType;
        }

        public bool IsKnownType(string typename)
        {
            return typeMap.ContainsKey(typename);
        }

        public void FindTransientVariables(string functionArguments, out Dictionary<string, string> transientArgs, out string[] transientArgsSetup, out string[] transientArgsCleanup)
        {
            transientArgs = new Dictionary<string, string>();
            List<string> setup = new List<string>(), cleanup = new List<string>();
            string[] args = StringHelper.SplitOnComma(functionArguments);
            for (int i = 0; i < args.Length; i++)
            {
                var s = args[i].Trim();
                if (string.IsNullOrEmpty(s))
                    continue;
                var varDecl = StringHelper.GetVariableDeclaration(s); // "const int*" "blah"
                addTransientVariable(varDecl, transientArgs, setup, cleanup);
            }
            transientArgsSetup = setup.ToArray();
            transientArgsCleanup = cleanup.ToArray();
        }

        protected void FindTransientVariables(StringBuilder sb, FuncAndArgs funcAndArgs, ref Dictionary<string, string> transientArgs, ref string[] transientArgsSetup, ref string[] transientArgsCleanup)
        {
            string functionArguments = funcAndArgs.Arguments;
            FindTransientVariables(functionArguments, out transientArgs, out transientArgsSetup, out transientArgsCleanup);
            foreach (var item in transientArgsSetup)
                sb.AppendLine(Indentation + item); // e.g. char** linkIdsChar = createAnsiStringArray(linkIds);
        }

        protected static bool FunctionReturnsValue(TypeAndName funcDef)
        {
            return (funcDef.TypeName.Trim() != "void");
        }

        private void addTransientVariable(TypeAndName varDecl, Dictionary<string, string> transientArgs, List<string> setup, List<string> cleanup)
        {
            string tname = varDecl.TypeName;
            string vname = varDecl.VarName;
            ArgConversion confInfo = null;
            if (transientArgConversion.ContainsKey(tname))
                confInfo = transientArgConversion[tname];
            else
            {
                confInfo = matchByRegex(transientArgConversion, tname);
            }
            if (confInfo == null) return;
            setup.Add(confInfo.GetSetup(vname));
            transientArgs.Add(vname, confInfo.GetTransientVarname(vname));
            cleanup.Add(confInfo.GetCleanup(vname));
        }

        private ArgConversion matchByRegex(Dictionary<string, ArgConversion> transientArgConversion, string tname)
        {
            foreach (var converter in transientArgConversion)
            {
                var key = converter.Key;
                if (key.StartsWith(".*")) // KLUDGE, If not doing that, trying with keys such as "char**" causes en exception.
                {
                    var rexpPtr = new Regex(converter.Key);
                    if (rexpPtr.IsMatch(tname))
                        return converter.Value;
                }
            }
            return null;
        }

        // CharacterVector nodeIds
        // char** nodeIdsChar = createAnsiStringArray(nodeIds);
        // freeAnsiStringArray(nodeIdsChar, nodeIds.length());
        private Dictionary<string, ArgConversion> transientArgConversion = new Dictionary<string, ArgConversion>();

        public void SetTransientArgConversion(string cArgType, string variablePostfix, string setupTemplate, string cleanupTemplate)
        {
            transientArgConversion[cArgType] = new ArgConversion(variablePostfix, setupTemplate, cleanupTemplate);
        }

        protected bool createWrappingFunctionSignature(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argumentConverterFunction)
        {
            string funcDef = funcAndArgs.Function + FunctionNamePostfix;
            if (!StringHelper.ParseTypeAndName(sb, funcDef, argumentConverterFunction)) return false;
            return AddFunctionArgs(sb, funcAndArgs, argumentConverterFunction);
        }

        protected string createWrappingFunctionBody(string line, FuncAndArgs funcAndArgs, StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc)
        {
            string result;
            sb.Append(FunctionBodyOpenDelimiter);
            bool ok = createWrapFuncBody(sb, funcAndArgs, argFunc);
            sb.Append(FunctionBodyCloseDelimiter);
            if (!ok)
                result = line;
            else
                result = sb.ToString();
            return result;
        }

        protected bool createWrapFuncBody(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc)
        {
            // We need to cater for cases where we need to create a transient variable then clean it, e.g.
            // char** c = transform((CharacterVector)cvec);
            // apiCall(c)
            // cleanup(c)

            Dictionary<string, string> transientArgs = null;
            string[] transientArgsSetup = null;
            string[] transientArgsCleanup = null;
            FindTransientVariables(sb, funcAndArgs, ref transientArgs, ref transientArgsSetup, ref transientArgsCleanup);

            var funcDef = GetTypeAndName(funcAndArgs.Function);
            bool returnsVal = FunctionReturnsValue(funcDef);
            // 	return XPtr<OpaquePointer>(new OpaquePointer(CloneModel(src->Get())));
            bool ok = CreateApiFunctionCall(sb, funcAndArgs, argFunc, transientArgs, funcDef, returnsVal);
            if (!ok) return false;
            CreateBodyCleanTransientVar(sb, funcAndArgs, transientArgsCleanup);
            CreateBodyReturnValue(sb, funcDef, returnsVal);
            return true;
        }

        protected static TypeAndName GetTypeAndName(string argString)
        {
            return new TypeAndName(argString);
        }

        public bool IsPointer(string typename)
        {
            foreach (string p in PointersEndsWithAny)
                if (typename.EndsWith(p)) return true;
            return false;
        }

        protected void CreateBodyCleanTransientVar(StringBuilder sb, FuncAndArgs funcAndArgs, string[] transientArgsCleanup)
        {
            foreach (var item in transientArgsCleanup)
                if (!string.IsNullOrEmpty(item))
                    sb.AppendLine(Indentation + item); // e.g. freeAnsiStringArray(nodeIdsChar, nodeIds.length());
        }

        protected bool CreateApiFunctionCall(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, string> transientArgs, TypeAndName funcDef, bool returnsVal)
        {
            sb.Append(Indentation);
            if (returnsVal) AppendReturnedValueDeclaration(sb);
            sb.Append(funcDef.VarName + ApiCallPostfix);
            if (!AddFunctionArgs(sb, funcAndArgs, argFunc, transientArgs)) return false;
            sb.Append(StatementSep);
            sb.Append(NewLineString);
            return true;
        }

        protected abstract void AppendReturnedValueDeclaration(StringBuilder sb);

        protected abstract void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal);

    }
}
