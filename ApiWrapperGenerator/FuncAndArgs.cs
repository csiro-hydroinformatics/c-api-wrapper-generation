using System;

namespace ApiWrapperGenerator
{
    public class FuncAndArgs
    {
        public FuncAndArgs(string s)
        {
            // At this point we'd have:
            //ModelRunner* CreateNewFromNetworkInfo(NodeInfo* nodes, int numNodes, LinkInfo* links, int numLinks);
            // or
            //SWIFT_API MODEL_SIMULATION_PTR CreateNewFromNetworkInfo(NODE_INFO_PTR nodes, int numNodes, LINK_INFO_PTR links, int numLinks);
            s = s.Replace(")", "");
            s = s.Replace(";", "");
            string[] funcAndArgs = s.Split(new[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            if (funcAndArgs.Length == 0) { Unexpected = true; return; }
            Function = funcAndArgs[0];
            if (funcAndArgs.Length == 1) { return; }
            Arguments = funcAndArgs[1];
            if (funcAndArgs.Length > 2) { Unexpected = true; }
        }
        public string Function = string.Empty;
        public string Arguments = string.Empty;
        public bool Unexpected = false;
    }
}
