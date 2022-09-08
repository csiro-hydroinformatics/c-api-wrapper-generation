using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ApiWrapperGenerator
{
    public class StringHelper
    {
        public static string NewLineString = "\n";

        public static string EnvNewLine = Environment.NewLine;

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

        public static string[] SplitToLines(string s)
        {
            // if (s.Contains("\r"))
            //     throw new FormatException("A line contains a carriage return character");
            string[] lines = s.Split(new string[] { EnvNewLine, NewLineString }, StringSplitOptions.None);
            //lines = (from l in lines select l.Trim()).ToArray();
            //lines = (from l in lines select l.Replace("  ", " ")).ToArray();
            return lines;
        }

        public static bool IsBlankLine(string s)
        {
            return s.Trim() == "";
        }

        public static string RemoveBlankLines(string s)
        {
            var lines = SplitToLines(s);
            var nonBlank = lines.Where(c => !StringHelper.IsBlankLine(c)).ToArray();
            return string.Join(EnvNewLine, nonBlank) + EnvNewLine;
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

        public static bool ReturnsDoublePtr(string funDef)
        {
            return (GetReturnedType(funDef) == "double*");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="argFunc">A method that processes the information contained in the argument 'arg'</param>
        /// <param name="transientArgs">A map from C API variable names to transient variables that are to be processed in lieu of 'arg'</param>
        /// <param name="args">The C API function arguments to process</param>
        /// <param name="start">start index of the arguments to process</param>
        /// <param name="end">last index of the arguments to process</param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static bool appendArgs(StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, TransientArgumentConversion> transientArgs, string[] args, int start, int end, string sep = ", ")
        {
            string arg;
            for (int i = start; i < end; i++)
            {
                arg = args[i];
                int lenBfore = sb.Length;
                if (!AddArgument(sb, argFunc, transientArgs, arg)) return false;
                if (i < (end - 1))
                    if (lenBfore != sb.Length)
                        sb.Append(sep);
            }
            return true;
        }

        /// <summary>
        /// Process a C API function argument according to a transformation
        /// </summary>
        /// <param name="sb">stringbuilder to append to</param>
        /// <param name="argFunc">A method that processes the information contained in the argument 'arg'</param>
        /// <param name="transientArgs">A map from C API variable names to transient variables that are to be processed in lieu of 'arg'</param>
        /// <param name="arg">The API argument to process, e.g. a string such as "char** varNames"</param>
        /// <returns></returns>
        public static bool AddArgument(StringBuilder sb, Action<StringBuilder, TypeAndName> argFunc, Dictionary<string, TransientArgumentConversion> transientArgs, string arg)
        {
            var typeAndName = StringHelper.GetVariableDeclaration(arg);
            if (typeAndName.Unexpected) return false;
            string vname = typeAndName.VarName;
            // No, it should be up to the caller to deal with the specificities of the call to check this, and add any additional particulars. At least had to do that for Python. 
            // if (transientArgs != null && transientArgs.ContainsKey(vname))
            // {
            //     sb.Append(transientArgs[vname].LocalVarname);
            //     return true;
            // }
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
