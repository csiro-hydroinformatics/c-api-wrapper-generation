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

        public bool IsPointer;

    }

    public abstract class BaseValueConversion
    {
        public const string WrappedApiArgumentName = "C_ARGNAME";
        public const string WrapperArgumentName = "RCPP_ARGNAME";
    }

    public class ReturnedValueConversion : BaseValueConversion
    {
        /// <summary>
        /// Template string specifying how to create the transient object. Uses WrappedApiArgumentName and WrapperArgumentName as placeholders
        /// </summary>
        public string ConversionTemplate;

        public string Apply(string variableName)
        {
            return ReplaceVariables(variableName, ConversionTemplate);
        }
        public string ReplaceVariables(string vname, string template)
        {
            return template
                .Replace(WrappedApiArgumentName, vname);
        }
    }

    /// <summary>
    /// Specifies how to convert an argument in the bindings type into an API argument for an API function call
    /// </summary>
    public class ArgConversion : BaseValueConversion
    {
        /// <summary>
        /// String to append to the base variable name to create a transient object to pass as argument
        /// </summary>
        string VariablePostfix;
        /// <summary>
        /// Template string specifying how to create the transient object. Uses WrappedApiArgumentName and WrapperArgumentName as placeholders
        /// </summary>
        string SetupTemplate;
        /// <summary>
        /// Template string specifying how to dispose of the transient object after the API call. Uses WrappedApiArgumentName and WrapperArgumentName as placeholders
        /// </summary>
        string CleanupTemplate;

        public bool IsPointer;

        public TransientArgumentConversion Apply(string variableName)
        {
            return new TransientArgumentConversion()
            {
                ApiVarname = variableName,
                LocalVarname = GetTransientVarname(variableName),
                LocalVarSetup = GetSetup(variableName),
                LocalVarCleanup = GetCleanup(variableName),
                IsPointer = this.IsPointer
            };
        }

        public ArgConversion(string variablePostfix, string setupTemplate, string cleanupTemplate, bool isPointer)
        {
            VariablePostfix = variablePostfix;
            SetupTemplate = setupTemplate;
            CleanupTemplate = cleanupTemplate;
            IsPointer = isPointer;
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
