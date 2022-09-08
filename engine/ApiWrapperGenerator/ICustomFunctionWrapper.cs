using System;
using System.Text;
using System.Collections.Generic;

namespace ApiWrapperGenerator
{
    /// <summary>
    /// Interface definition for finding functions that need a 
    /// more advanced conversion, considering them "as a whole" when parsing.
    /// </summary>
    public interface ICustomFunctionWrapper
    {
        string CreateWrapper(string funcDef, bool declarationOnly);
        bool IsMatch(string funcDef);
    }

    /// <summary>
    /// A default implementation for finding functions that need a 
    /// more advanced conversion, considering them "as a whole" when parsing.
    /// </summary>
    public class CustomFunctionWrapperImpl : ICustomFunctionWrapper
    {
        public CustomFunctionWrapperImpl()
        {
            StatementSep = ";";
        }

        public string Template;
        public string Docstring = "%WRAPFUNCTIONDOCSTRING%";
        public string Argstvar = "%ARGS%";
        public string Wrapargstvar = "%WRAPARGS%";
        public string Functvar = "%FUNCTION%";
        public string Wrapfunctvar = "%WRAPFUNCTION%";
        public string Transargtvar = "%TRANSARGS%";
        public string Transargcleantvar = "%CLEANTRANSARGS%";
        /// Number of arguments removed used for the %ARGS% marker. If API args (a, b, c) and  RemovedLastArgs == 1, then template replacement with write a, b
        public int RemovedLastArgs = 1;

        public bool TransientArgsAppendNewline = false;

        public Action<StringBuilder, TypeAndName> ApiArgToWrappingLang = null;
        public Action<StringBuilder, TypeAndName> ApiCallArgument = null;
        public Action<StringBuilder, TypeAndName> TransientArgsCreation = null;
        public Action<StringBuilder, TypeAndName> TransientArgsCleanup = null;
        public Func<FuncAndArgs, string> ApiSignatureToDocString = null;


        public string FunctionNamePostfix = "";
        public string CalledFunctionNamePostfix = "";
        public string CalledFunctionNamePrefix = "";

        public string CreateWrapper(string funDef, bool declarationOnly)
        {
            string funcName = StringHelper.GetFuncName(funDef);
            string wrapFuncName = funcName + this.FunctionNamePostfix;
            string calledfuncName = CalledFunctionNamePrefix + funcName + this.CalledFunctionNamePostfix;
            var fullResult = Template
                .Replace(Wrapargstvar, WrapArgsDecl(funDef, 0, 0))
                .Replace(Argstvar, FuncCallArgs(funDef, 0, 0, false))
                .Replace(Wrapfunctvar, wrapFuncName)
                .Replace(Functvar, calledfuncName)
                .Replace(Transargtvar, TransientArgs(funDef, 0, 0))
                .Replace(Transargcleantvar, TransientArgsDispose(funDef, 0, 0))
                .Replace(Docstring, GenerateDocString(funDef))
                // cater for cases where templates with (%WRAPARGS%, IntPtr size) if %WRAPARGS% is empty
                .Replace("(,", "(")
                .Replace(",,", ",")
                .Replace(", ,", ",")
                .Replace(",)", ")")
                ;

            // for e.g. python avoid multiple newlines.
            fullResult = fullResult.Replace("\n\r\n", "\r\n");
            fullResult = fullResult.Replace("\r\n", StringHelper.EnvNewLine);

            if (declarationOnly)
                return (getDeclaration(fullResult)); // HACK - brittle as assumes the template header is the only thing on the first line.
            else
                return fullResult;
        }

        public string StatementSep { get; set; }

        private string getDeclaration(string fullResult)
        {
            string[] newLines = new string[] { StringHelper.EnvNewLine, StringHelper.NewLineString };
            var lines = fullResult.Split(newLines, StringSplitOptions.RemoveEmptyEntries);
            int firstValidIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim() == string.Empty)
                    lines[i] = string.Empty;
                else
                    firstValidIndex = i;
            }
            return lines[firstValidIndex] + StatementSep;
        }

        public bool IsMatch(string funDef)
        {
            if (IsMatchFunc == null) return false;
            return IsMatchFunc(funDef);
        }

        public Func<string, bool> IsMatchFunc = null;

        // Below are more tricky ones, not yet fully fleshed out support.

        private string WrapArgsDecl(string funDef, int start, int offsetLength)
        {
            if (ApiArgToWrappingLang == null) return string.Empty;
            return ProcessFunctionArguments(funDef, start, offsetLength, ApiArgToWrappingLang);
        }

        private string GenerateDocString(string funDef)
        {
            if (ApiSignatureToDocString == null)
                return string.Empty;
            else
                return ApiSignatureToDocString(new FuncAndArgs(funDef));
        }

        private string TransientArgs(string funDef, int start, int offsetLength)
        {
            if (TransientArgsCreation == null) return string.Empty;
            string result = ProcessFunctionArguments(funDef, start, offsetLength, TransientArgsCreation, appendSeparator: this.TransientArgsAppendNewline, sep: StringHelper.EnvNewLine);
            AppendSeparatorIfNeeded(StringHelper.EnvNewLine, ref result);
            return result;
        }

        private string TransientArgsDispose(string funDef, int start, int offsetLength)
        {
            if (TransientArgsCleanup == null) return string.Empty;
            string result = ProcessFunctionArguments(funDef, start, offsetLength, TransientArgsCleanup, appendSeparator: this.TransientArgsAppendNewline, sep: StringHelper.EnvNewLine);
            AppendSeparatorIfNeeded(StringHelper.EnvNewLine, ref result);
            return result;
        }

        private string FuncCallArgs(string funDef, int start, int offsetLength, bool appendSeparator)
        {
            if (ApiCallArgument == null) return string.Empty;
            return ProcessFunctionArguments(funDef, start, offsetLength, ApiCallArgument, appendSeparator);
        }

        private string ProcessFunctionArguments(string funDef, int start, int offsetLength, Action<StringBuilder, TypeAndName> argFunc, bool appendSeparator = false, string sep = ", ")
        {
            StringBuilder sb = new StringBuilder();
            var args = StringHelper.GetFunctionArguments(funDef);
            int end = args.Length - this.RemovedLastArgs - offsetLength;
            var transientArgs = new Dictionary<string, TransientArgumentConversion>();
            StringHelper.appendArgs(sb, argFunc, transientArgs, args, 0, end, sep);
            if (appendSeparator && (end > start))
                AppendSeparatorIfNeeded(sep, sb);
            var s = sb.ToString();
            if (sep == StringHelper.EnvNewLine || sep == StringHelper.NewLineString)
                return StringHelper.RemoveBlankLines(s);
            else
                return s;
        }

        private static void AppendSeparatorIfNeeded(string sep, StringBuilder sb)
        {
            BaseApiConverter.AppendSeparatorIfNeeded(sep, sb);
        }

        private static void AppendSeparatorIfNeeded(string sep, ref string theString)
        {
            BaseApiConverter.AppendSeparatorIfNeeded(sep, ref theString);
        }
    }
}
