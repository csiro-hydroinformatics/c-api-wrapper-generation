namespace ApiWrapperGenerator
{
    public class TransientArgumentConversion
    {
        /// <summary>
        /// Name of the variable detected in the API
        /// </summary>
        public string LocalVarname;
        /// <summary>
        /// Name of the variable created in the body of the wrapper function
        /// </summary>
        public string ApiVarname;
        /// <summary>
        /// Statements setting up the local variable prior to the API call
        /// </summary>
        public string LocalVarSetup;
        /// <summary>
        /// Statements disposing of the local variable prior to the API call
        /// </summary>
        public string LocalVarCleanup;
    }

    public class ArgConversion
    {
        string VariablePostfix;
        string SetupTemplate;
        string CleanupTemplate;

        const string WrappedApiArgumentName = "C_ARGNAME";
        const string WrapperArgumentName = "RCPP_ARGNAME";

        public TransientArgumentConversion Apply(string variableName)
        {
            return new TransientArgumentConversion()
            {
                ApiVarname = variableName,
                LocalVarname = GetTransientVarname(variableName),
                LocalVarSetup = GetSetup(variableName),
                LocalVarCleanup = GetCleanup(variableName)
            };
        }

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
