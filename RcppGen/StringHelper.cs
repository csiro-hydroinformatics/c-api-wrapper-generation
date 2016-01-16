using System;
using System.Collections.Generic;
using System.Text;

namespace ApiWrapperGenerator
{
    public class StringHelper
    {
        public static string NewLineString = "\n";

        public static string Concat(string[] elemts, int start, int count, string sep = " ")
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

        public static string GetReturnedType(string funDef)
        {
            return GetFunctionTypeAndName(funDef).TypeName;
        }

        public static TypeAndName GetFunctionTypeAndName(string funDef)
        {
            FuncAndArgs funcAndArgs = GetFuncDeclAndArgs(funDef);
            return GetVariableDeclaration(funcAndArgs.Function);
        }

        public static string GetFuncName(string funDef)
        {
            TypeAndName typeAndName = GetFunctionTypeAndName(funDef);
            return typeAndName.VarName;
        }

        public static string[] GetFunctionArguments(string funDef)
        {
            FuncAndArgs funcAndArgs = GetFuncDeclAndArgs(funDef);
            return SplitOnComma(funcAndArgs.Arguments);
        }

        public static string[] SplitOnComma(string functionArguments)
        {
            string[] args = functionArguments.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); //NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks
            return args;
        }

        public static TypeAndName GetVariableDeclaration(string argString)
        {
            return new TypeAndName(argString);
        }

        public static FuncAndArgs GetFuncDeclAndArgs(string line)
        {
            return new FuncAndArgs(line);
        }

        public static bool ReturnsCharPP(string funDef)
        {
            return (GetReturnedType(funDef) == "char**");
        }

        public static bool appendArgs(StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, string> transientArgs, string[] args, int start, int end, string sep = ", ")
        {
            string arg;
            for (int i = start; i < end; i++)
            {
                arg = args[i];
                if (!addArgument(sb, argFunc, transientArgs, arg)) return false;
                if (i < (end - 1))
                    sb.Append(sep);
            }
            return true;
        }

        public static bool addArgument(StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, string> transientArgs, string arg)
        {
            var typeAndName = StringHelper.GetVariableDeclaration(arg);
            if (typeAndName.Unexpected) return false;
            string vname = typeAndName.VarName;
            if (transientArgs != null && transientArgs.ContainsKey(vname))
            {
                sb.Append(transientArgs[vname]);
                return true;
            }
            return ParseTypeAndName(sb, arg, argFunc);
        }

        public static bool ParseTypeAndName(StringBuilder sb, string argString, Action<StringBuilder, TypeAndName> fun = null)
        {
            // argString could be something like:
            // double x
            // const char* s
            // ModelRunner * s
            var typeAndName = StringHelper.GetVariableDeclaration(argString);

            if (typeAndName.Unexpected) return false;
            fun(sb, typeAndName);
            return true;
        }


    }
}
