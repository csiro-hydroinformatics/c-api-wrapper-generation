using System;
using System.Text;

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
        public string argstvar = "%ARGS%";
        public string wrapargstvar = "%WRAPARGS%";
        public string functvar = "%FUNCTION%";
        public string wrapfunctvar = "%WRAPFUNCTION%";
        public string transargtvar = "%TRANSARGS%";


        public string FunctionNamePostfix = "";
        public string CalledFunctionNamePostfix = "";

        public string CreateWrapper(string funDef, bool declarationOnly)
        {
            string funcName = StringHelper.GetFuncName(funDef);
            string wrapFuncName = funcName + this.FunctionNamePostfix;
            string calledfuncName = funcName + this.CalledFunctionNamePostfix;
            var fullResult = Template
                .Replace(wrapargstvar, WrapArgsDecl(funDef, 0, 0))
                .Replace(argstvar, FuncCallArgs(funDef, 0, 0))
                .Replace(wrapfunctvar, wrapFuncName)
                .Replace(functvar, calledfuncName)
                .Replace(transargtvar, TransientArgs(funDef, 0, 0));

            if (declarationOnly)
                return (getDeclaration(fullResult)); // HACK - brittle as assumes the template header is the only thing on the first line.
            else
                return fullResult;
        }

        public string StatementSep { get; set; }

        private string getDeclaration(string fullResult)
        {
            string[] newLines = new string[] { Environment.NewLine, "\n" };
            var lines = fullResult.Split(newLines, StringSplitOptions.RemoveEmptyEntries);
            return lines[0] + StatementSep;
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
            if (ApiArgToRcpp == null) return string.Empty;
            return ProcessFunctionArguments(funDef, start, offsetLength, ApiArgToRcpp);
        }

        public Action<StringBuilder, TypeAndName> ApiArgToRcpp = null;
        public Action<StringBuilder, TypeAndName> ApiCallArgument = null;
        public Action<StringBuilder, TypeAndName> TransientArgsCreation = null;

        private string TransientArgs(string funDef, int start, int offsetLength)
        {
            if (TransientArgsCreation == null) return string.Empty;
            string result = ProcessFunctionArguments(funDef, start, offsetLength, TransientArgsCreation, appendSeparator: true, sep: StringHelper.NewLineString);
            result += StringHelper.NewLineString;
            return result;
        }

        private string FuncCallArgs(string funDef, int start, int offsetLength)
        {
            if (ApiCallArgument == null) return string.Empty;
            return ProcessFunctionArguments(funDef, start, offsetLength, ApiCallArgument, appendSeparator: true);
        }

        private string ProcessFunctionArguments(string funDef, int start, int offsetLength, Action<StringBuilder, TypeAndName> argFunc, bool appendSeparator = false, string sep = ", ")
        {
            StringBuilder sb = new StringBuilder();
            var args = StringHelper.GetFunctionArguments(funDef);
            int end = args.Length - 1 - offsetLength;
            StringHelper.appendArgs(sb, argFunc, null, args, 0, end, sep);
            if (appendSeparator && (end > start)) sb.Append(sep);
            return sb.ToString();
        }
    }
}
