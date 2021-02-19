namespace ApiWrapperGenerator
{
    /// <summary>
    /// An interface definition for objects that parse a 
    /// C API line by line and convert to related functions.
    /// </summary>
    public interface IApiConverter
    {
        string ConvertLine(string line);

        /// <summary>
        /// Get the preamble of a generated file, usually a warning comment not to modify it.
        /// </summary>
        string GetPreamble();

        /// <summary>
        /// Get the postamble for a generated file
        /// </summary>
        string GetPostamble();
    }
}
