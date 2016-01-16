using System;

namespace ApiWrapperGenerator
{
    public class TypeAndName
    {
        public TypeAndName(string argString)
        {
            // argString could be something like:
            // double x
            // const char* s
            // ModelRunner * s
            var typeAndName = argString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);//"ModelRunner*" "CreateNewFromNetworkInfo"
                                                                                                           // cater for things like const char* s:
            if (typeAndName.Length > 2)
                typeAndName = new[]{
                    StringHelper.Concat(typeAndName, 0, typeAndName.Length-1),
                    typeAndName[typeAndName.Length-1]};
            if (typeAndName.Length == 2)
            {
                TypeName = typeAndName[0].Trim();
                VarName = typeAndName[1].Trim();
            }
            else
                Unexpected = true;
        }
        public string TypeName = string.Empty;
        public string VarName = string.Empty;
        public bool Unexpected = false;
    }
}
