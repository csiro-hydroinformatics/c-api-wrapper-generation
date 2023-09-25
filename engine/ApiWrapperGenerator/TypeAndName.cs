using System;
using System.Collections.Generic;

namespace ApiWrapperGenerator
{
    public class TypeAndName
    {

        private static List<string> reservedWords = new List<string>();
        /// <summary>
        /// List of words that are reserved in the target wrapper language, e.g. lambda in python
        /// </summary>
        public static List<string> ReservedWords
        {
            get { return reservedWords; }
            set { reservedWords = value; }
        }
        
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
                VarName = checkReservedWords(typeAndName[1].Trim());
            }
            else
                Unexpected = true;
        }

        private string checkReservedWords(string v)
        {
            if (TypeAndName.reservedWords.Count == 0) return v;
            string y = v;
            foreach (var x in TypeAndName.ReservedWords)
            {
                if (x == v)
                {
                    y = v + "_var";
                    break;
                }
            }
            return y;
        }

        public string TypeName = string.Empty;
        public string VarName = string.Empty;
        public bool Unexpected = false;
    }
}
