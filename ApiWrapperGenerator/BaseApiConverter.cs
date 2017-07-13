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
            ArgListOpenDelimiter = "(";
            ArgListCloseDelimiter = ")";
            FunctionBodyOpenDelimiter = "{";
            FunctionBodyCloseDelimiter = "}";
            StatementSep = ";";
            ApiCallPostfix = string.Empty;
            PointersEndsWithAny = new string[] { CPtr, "_PTR" };
            CallGetMethod = "->Get()";
            CastToOpaquePtrPtr = "(void**)";
            Indentation = "    ";
            UniformIndentationCount = 0;
            ApiCallOpenParenthesis = true; // a kludge switch to cater for matlab's calllib
        }

        public const string CPtr = "*";

        public string Indentation { get; set; }
        public string TwoIndentations { get { return Indentation + Indentation; } }
        public string ThreeIndentations { get { return Indentation + Indentation + Indentation; } }

        private int uniformIndentationCount;
        protected string UniformIndentation
        {
            get;
            private set;
        }
        public int UniformIndentationCount
        {
            get { return uniformIndentationCount; }
            set
            {
                uniformIndentationCount = value;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < uniformIndentationCount; i++)
                {
                    sb.Append(Indentation);
                }
                UniformIndentation = sb.ToString();
            }
        }

        public string BodyLineTermination { get { return StatementSep + NewLineString; } }
        public string BodyLineIdentation { get { return UniformIndentation + Indentation; } }
        public string BodyLineOpenFunctionDelimiter { get { return UniformIndentation + FunctionBodyOpenDelimiter + NewLineString; } }
        public string BodyLineCloseFunctionDelimiter { get { return UniformIndentation + FunctionBodyCloseDelimiter + NewLineString; } }

        public string ArgListOpenDelimiter { get; set; }
        public string ArgListCloseDelimiter { get; set; }
        public string FunctionBodyOpenDelimiter { get; set; }
        public string FunctionBodyCloseDelimiter { get; set; }

        public string ApiCallPostfix { get; set; }
        public string ApiCallPrefix { get; set; }

        public string StatementSep { get; set; }

        public string CreateXptrObjRefFunction { get; set; }
        public string GetXptrFromObjRefFunction { get; set; }

        public string FunctionNamePostfix { get; set; }

        public string[] PointersEndsWithAny { get; set; }

        public string NewLineString { get; set; }

        public bool DeclarationOnly { get; set; }

        public string CallGetMethod { get; set; }

        public string CastToOpaquePtrPtr { get; set; }

        public string PrependOutputFile { get; set; }

        public string AppendOutputFile { get; set; }

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

        protected static bool IsCharPtr(string typename)
        {
            return (typename.EndsWith("char*"));
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
                if (t.EndsWith(ptrTermination + CPtr))
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

        public string GetPostamble()
        {
            return AppendOutputFile;
        }

        public void ClearCustomWrappers()
        {
            customWrappers.Clear();
        }

        public void AddCustomWrapper(CustomFunctionWrapperImpl cw)
        {
            customWrappers.Add(cw);
        }

        /// <summary> Adds the arguments of a function in the generated code </summary>
        ///
        /// <param name="sb">            The sb.</param>
        /// <param name="funcAndArgs">   The function and arguments.</param>
        /// <param name="argFunc">       a function that generates the parameter name and type in the target language, based on the TypeAndName info from the source language</param>
        /// <param name="transientArgs"> (Optional) The transient arguments.</param>
        ///
        /// <returns> True if it succeeds, false if it fails.</returns>
        protected bool AddFunctionArgs(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, TransientArgumentConversion> transientArgs = null, bool openParenthesis=true)
        {
            if(openParenthesis) // Kludge for e.g. matlab calllib('mylib','mufunc',
                sb.Append(ArgListOpenDelimiter);
            string[] args = GetFuncArguments(funcAndArgs);
            if (args.Length > 0)
            {
                int start = 0, end = args.Length - 1;
                if (!StringHelper.appendArgs(sb, argFunc, transientArgs, args, start, end)) return false;
                if (end > start)
                    sb.Append(", ");
                string arg = args[args.Length - 1];
                if (!StringHelper.AddArgument(sb, argFunc, transientArgs, arg)) return false;
            }
            sb.Append(ArgListCloseDelimiter);
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

        public Dictionary<string, TransientArgumentConversion> FindTransientVariables(string functionArguments)
        {
            var transientArgs = new Dictionary<string, TransientArgumentConversion>();
            string[] args = StringHelper.SplitOnComma(functionArguments);
            for (int i = 0; i < args.Length; i++)
            {
                var s = args[i].Trim();
                if (string.IsNullOrEmpty(s))
                    continue;
                var varDecl = StringHelper.GetVariableDeclaration(s); // "const int*" "blah"
                addTransientVariable(varDecl, transientArgs);
            }
            //transientArgsSetup = setup.ToArray();
            //transientArgsCleanup = cleanup.ToArray();
            return transientArgs;
        }

        protected Dictionary<string, TransientArgumentConversion> FindTransientVariables(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            string functionArguments = funcAndArgs.Arguments;
            return FindTransientVariables(functionArguments);
        }

        protected static bool FunctionReturnsValue(TypeAndName funcDef)
        {
            return (funcDef.TypeName.Trim() != "void");
        }

        private void addTransientVariable(TypeAndName varDecl, Dictionary<string, TransientArgumentConversion> transientArgs)
        {
            string tname = varDecl.TypeName;
            string vname = varDecl.VarName;
            ArgConversion conv = FindConverter(tname);
            if (conv == null) return;
            var t = conv.Apply(vname);
            transientArgs[vname] = t;
        }

        private ArgConversion FindConverter(string tname)
        {
            ArgConversion conv = null;
            if (transientArgConversion.ContainsKey(tname))
                conv = transientArgConversion[tname];
            else
            {
                conv = matchByRegex(transientArgConversion, tname);
            }

            return conv;
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

        protected bool createWrappingFunctionSignature(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argumentConverterFunction, string functionNamePostfix)
        {
            string funcDef = funcAndArgs.Function + functionNamePostfix;
            sb.Append(UniformIndentation);//  indentation in "         public void Blah();"
            if (!StringHelper.ParseTypeAndName(sb, funcDef, argumentConverterFunction)) return false;
            bool b = AddFunctionArgs(sb, funcAndArgs, argumentConverterFunction);
            sb.Append(NewLineString);
            return b;
        }

        protected string createWrappingFunctionBody(string line, FuncAndArgs funcAndArgs, StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc)
        {
            string result;
            sb.Append(BodyLineOpenFunctionDelimiter);
            AddInFunctionDocString(sb, funcAndArgs);
            bool ok = createWrapFuncBody(sb, funcAndArgs, argFunc);
            sb.Append(BodyLineCloseFunctionDelimiter);
            if (!ok)
                result = line;
            else
                result = sb.ToString();
            return result;
        }

        /// <summary> Placeholder to add things such as Python docstrings.</summary>
        ///
        /// <param name="sb">          The sb.</param>
        /// <param name="funcAndArgs"> The function and arguments.</param>
        protected virtual void AddInFunctionDocString(StringBuilder sb, FuncAndArgs funcAndArgs)
        {
            // nothing here.
        }

        protected bool createWrapFuncBody(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc)
        {
            // We need to cater for cases where we need to create a transient variable then clean it, e.g.
            // char** c = transform((CharacterVector)cvec);
            // apiCall(c)
            // cleanup(c)

            var tConv = FindTransientVariables(sb, funcAndArgs);
            var funcDef = GetTypeAndName(funcAndArgs.Function);
            bool returnsVal = FunctionReturnsValue(funcDef);
            CreateBodyCreateTransientVar(sb, tConv);
            bool ok = CreateApiFunctionCall(sb, funcAndArgs, argFunc, tConv, funcDef, returnsVal);
            if (!ok) return false;
            CreateBodyCleanTransientVar(sb, tConv);
            CreateBodyReturnValue(sb, funcDef, returnsVal);
            return true;
        }

        protected static TypeAndName GetTypeAndName(string argString)
        {
            return new TypeAndName(argString);
        }

        protected static string QuotedString(string s, string quote = "\"")
        {
            return quote + s + quote;
        }

        public bool IsPointer(string typename)
        {
            foreach (string p in PointersEndsWithAny)
                if (typename.EndsWith(p)) return true;
            return false;
        }

        private void CreateBodyCreateTransientVar(StringBuilder sb, Dictionary<string, TransientArgumentConversion> tConv)
        {
            foreach (var item in tConv)
                // e.g. char** linkIdsChar = createAnsiStringArray(linkIds);
                AddIfExistingStatement(sb, item.Value.LocalVarSetup);
        }
        protected void CreateBodyCleanTransientVar(StringBuilder sb, Dictionary<string, TransientArgumentConversion> tConv)
        {
            foreach (var item in tConv)
                // e.g. freeAnsiStringArray(nodeIdsChar, nodeIds.length());
                AddIfExistingStatement(sb, item.Value.LocalVarCleanup);
        }

        protected void AddBodyLine(StringBuilder sb, string statement)
        {
            if (!string.IsNullOrEmpty(statement))
                sb.Append(BodyLineIdentation + statement + BodyLineTermination);
        }

        protected void AddLine(StringBuilder sb, string statement)
        {
            if (!string.IsNullOrEmpty(statement))
                sb.Append(UniformIndentation + statement + NewLineString);
        }

        private void AddIfExistingStatement(StringBuilder sb, string statement)
        {
            if (!string.IsNullOrEmpty(statement))
            {
                sb.Append(UniformIndentation);
                AddLine(sb, Indentation + statement);
            }
        }

        public bool ApiCallOpenParenthesis { get; set; }

        protected bool CreateApiFunctionCall(StringBuilder sb, FuncAndArgs funcAndArgs, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, TransientArgumentConversion> transientArgs, TypeAndName funcDef, bool returnsVal)
        {
            sb.Append(UniformIndentation);
            sb.Append(Indentation);
            if (returnsVal) AppendReturnedValueDeclaration(sb);
            CreateApiFunctionCallFunction(sb, funcDef);
            if (!AddFunctionArgs(sb, funcAndArgs, argFunc, transientArgs, ApiCallOpenParenthesis)) return false;
            sb.Append(StatementSep);
            sb.Append(NewLineString);
            return true;
        }

        protected virtual void CreateApiFunctionCallFunction(StringBuilder sb, TypeAndName funcDef)
        {
            sb.Append(ApiCallPrefix + funcDef.VarName + ApiCallPostfix);
        }

        protected abstract void CreateBodyReturnValue(StringBuilder sb, TypeAndName funcDef, bool returnsVal);

        public string ReturnedValueDeclarationKeyword = "";

        protected void AppendReturnedValueDeclaration(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(ReturnedValueDeclarationKeyword))
                sb.Append(ReturnedValueDeclarationKeyword + " ");
            sb.Append(ReturnedValueVarname);
            sb.Append(" "); sb.Append(AssignmentSymbol); sb.Append(" ");
        }

        protected void TransientArgsCreation(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(CreateTransientApiArgument(typeAndName));
        }

        protected void TransientArgsCleanup(StringBuilder sb, TypeAndName typeAndName)
        {
            sb.Append(CleanupTransientApiArgument(typeAndName));
        }

        public string CreateTransientApiArgument(string apiArgument)
        {
            return CreateTransientApiArgument(new TypeAndName(apiArgument));
        }

        public string CreateTransientApiArgument(TypeAndName typeAndName)
        {
            TransientArgumentConversion t = FindTransientArgConversion(typeAndName);
            if (t == null) return string.Empty;
            return t.LocalVarSetup;
        }

        public string CleanupTransientApiArgument(string apiArgument)
        {
            return CleanupTransientApiArgument(new TypeAndName(apiArgument));
        }

        public string CleanupTransientApiArgument(TypeAndName typeAndName)
        {
            TransientArgumentConversion t = FindTransientArgConversion(typeAndName);
            if (t == null) return string.Empty;
            return t.LocalVarCleanup;
        }

        protected TransientArgumentConversion FindTransientArgConversion(string typename, string varname)
        {
            TransientArgumentConversion t;
            ArgConversion conv = FindConverter(typename);
            return (conv == null ? null : conv.Apply(varname));
        }

        protected TransientArgumentConversion FindTransientArgConversion(TypeAndName typeAndName)
        {
            return FindTransientArgConversion(typeAndName.TypeName, typeAndName.VarName);
        }

        public static void AppendSeparatorIfNeeded(string sep, StringBuilder sb)
        {
            if (sb.Length == 0)
                return;
            var currentEnd = sb[sb.Length - 1].ToString();
            if (currentEnd != sep)
                sb.Append(sep);
        }

        public static void AppendSeparatorIfNeeded(string sep, ref string theString)
        {
            if (theString.Length == 0)
                return;
            if (!theString.EndsWith(sep))
                theString += sep;
        }
    }
}
