using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ApiWrapperGenerator
{
    public class CodeFileFilter
    {
        protected CodeFileFilter()
        {
        }

        public string[] Filter(string inputFile)
        {
            string input = File.ReadAllText(inputFile);
            return FilterInput(input);
        }

        public string[] FilterInput(string input)
        {
            var filteredInput = OnLoadingRawInputFile(input);
            return FindMatchingLines(filteredInput);
        }

        protected virtual string OnLoadingRawInputFile(string rawInput)
        {
            return rawInput;
        }

        public string[] FindMatchingLines(string input)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            List<string> output = new List<string>();
            using (var tr = new StringReader(input))
            {
                string line = "";
                while (line != null)
                {
                    line = line.Trim();
                    if (IsMatch(line))
                    {
                        line = prepareInLine(line);
                        output.Add(line);
                    }
                    line = tr.ReadLine();
                }
            }
            return output.ToArray();
        }

        protected string prepareInLine(string line)
        {
            string s = line.Replace("\t", " ");
            s = s.Trim();
            s = removeToRemove(s);
            s = s.Trim();
            s = preprocessPointers(s);
            return s;
        }

        protected static string preprocessPointers(string s)
        {
            // Make all pointers types without blanks
            var rexpPtr = new Regex(" *\\*");
            s = rexpPtr.Replace(s, BaseApiConverter.CPtr);
            return s;
        }

        protected string removeToRemove(string s)
        {
            foreach (var r in ToRemove)
                s = s.Replace(r, "");
            return s;
        }

        public bool IsMatch(string line)
        {
            line = line.Trim();
            if (StartsWithExcluded(line)) return false;
            bool match = false;
            if (ContainsAny.Length > 0)
            {
                foreach (string p in ContainsAny)
                    match = match || line.Contains(p);
                if (!match) return false;
            }
            match = true;
            foreach (string p in ContainsNone)
                if (line.Contains(p)) return false;

            return match;
        }

        protected bool StartsWithExcluded(string line)
        {
            foreach (string p in NotStartsWith)
                if (line.StartsWith(p)) return true;
            return false;
        }

        public string[] NotStartsWith { get; set; }

        public string[] ToRemove { get; set; }

        public string[] ContainsAny { get; set; }

        public string[] ContainsNone { get; set; }

    }

    public class HeaderFilter : CodeFileFilter
    {
        public HeaderFilter()
        {
            ContainsAny = new string[] { "SWIFT_API" };
            ToRemove = new string[] { "SWIFT_API" };
            ContainsNone = new string[] { "#define" };
            NotStartsWith = new string[] { "//" };
        }

    }

    public class RcppExportedCppFunctions : CodeFileFilter
    {
        public RcppExportedCppFunctions()
        {
            ContainsAny = new string[] { "[[Rcpp::export]] " };
            ToRemove = new string[] { "[[Rcpp::export]] " };
            ContainsNone = new string[] { "#define" };
            NotStartsWith = new string[] { "//" };
        }

        protected override string OnLoadingRawInputFile(string rawInput)
        {
            var x = rawInput.Replace("// [[Rcpp::export]]\n\r", "[[Rcpp::export]] ");
            x = x.Replace("// [[Rcpp::export]]\r\n", "[[Rcpp::export]] ");
            x = x.Replace("// [[Rcpp::export]]\n", "[[Rcpp::export]] ");
            return x;
        }
    }
}