namespace ApiWrapperGenerator
{
    /// <summary>
    /// An interface definition for objects that parse a 
    /// C API line by line and convert to related functions.
    /// </summary>
    public interface IApiConverter
    {
        string ConvertLine(string line);
        string GetPreamble();
    }
}
