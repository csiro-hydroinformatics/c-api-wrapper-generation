using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApiWrapperGenerator
{
    public class WrapperGenerator
    {
        public WrapperGenerator(IApiConverter converter)
        {
            this.converter = converter;
            this.filter = new HeaderFilter();
        }
        public WrapperGenerator(IApiConverter converter, CodeFileFilter filter)
        {
            this.converter = converter;
            this.filter = filter;
        }

        IApiConverter converter;
        CodeFileFilter filter;

        public void CreateWrapperHeader(string inputFile, string outputFile)
        {
            string[] lines = filter.Filter(inputFile);
            StringBuilder sb = new StringBuilder();
            sb.Append(converter.GetPreamble());
            string[] outputlines = Convert(lines);
            for (int i = 0; i < outputlines.Length; i++)
            {
                sb.Append(outputlines[i]);
            }
            sb.Append(converter.GetPostamble());
            string output = sb.ToString();
            File.WriteAllText(outputFile, output);
        }

        public string[] Convert(string[] lines)
        {
            //SWIFT_API ModelRunner * CloneModel(ModelRunner * src);
            //SWIFT_API ModelRunner * CreateNewFromNetworkInfo(NodeInfo * nodes, int numNodes, LinkInfo * links, int numLinks);
            List<string> converted = new List<string>();

            //sb.Append(PrependOutputFile);
            foreach (string lineRaw in lines)
            {
                string line = lineRaw.Trim();
                string convertedLine = converter.ConvertLine(line);
                converted.Add(convertedLine);
            }
            return converted.ToArray();
        }
    }
}
