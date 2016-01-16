namespace ApiWrapperGenerator
{
    public class ArgConversion
    {
        string VariablePostfix;
        string SetupTemplate;
        string CleanupTemplate;

        const string WrappedApiArgumentName = "C_ARGNAME";
        const string WrapperArgumentName = "RCPP_ARGNAME";

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
            return template
                .Replace(WrappedApiArgumentName, GetTransientVarname(vname))
                .Replace(WrapperArgumentName, vname);
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

}
