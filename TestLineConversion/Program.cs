using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ApiWrapperGenerator;

namespace TestLineConversion
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.Error.WriteLine("Usage:");
                Console.Error.WriteLine("TestLineConversion \"SWIFTAPI void blah();\" wrappertype");
                return;
            }
            string gen = args[1];
            //TestStrings();
            if(gen == "cs")
                TestCsWrpGen(args);
            else if (gen == "m")
                TestMatlabWrpGen(args);
            else if (gen == "r")
                TestRWrpGen(args);
            else
                Console.Error.WriteLine("Unknown option " + gen);
        }

        private static IntPtr TestStrings()
        {
            string[] test = { "a", "bb" };
            IntPtr charpp = Marshal.AllocHGlobal(test.Length * IntPtr.Size);

            for (int i = 0; i < test.Length; i++)
            {
                int offset = i * IntPtr.Size;
                IntPtr p = Marshal.StringToHGlobalAnsi(test[i]);
                Marshal.WriteIntPtr(charpp, offset, p);
            }

            string[] reread = new string[2];
            for (int i = 0; i < test.Length; i++)
            {
                int offset = i * IntPtr.Size;
                IntPtr p = Marshal.ReadIntPtr(charpp, offset);
                //IntPtr pointer = IntPtr.Add(p, Marshal.SizeOf(typeof(VECTOR_SEXPREC)));
                reread[i] = Marshal.PtrToStringAnsi(p);
            }

            for (int i = 0; i < test.Length; i++)
            {
                int offset = i * IntPtr.Size;
                IntPtr p = Marshal.ReadIntPtr(charpp, offset);
                Marshal.FreeHGlobal(p);
            }
            Marshal.FreeHGlobal(charpp);

            return charpp;
        }

        private static void TestCsWrpGen(string[] args)
        {
            var gen = new CsharpApiWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            ProcessTestLine(args, gen);
        }

        private static void ProcessTestLine(string[] args, IApiConverter gen)
        {
            HeaderFilter filter = createFilter();
            string apiLine = args[0];
            apiLine = filter.FilterInput(apiLine)[0];
            var w = new WrapperGenerator(gen, filter);
            var result = w.Convert(new string[] { apiLine });
            Console.WriteLine(result[0]);
        }

        private static void TestMatlabWrpGen(string[] args)
        {
            var gen = new MatlabApiWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            ProcessTestLine(args, gen);
        }

        private static void TestRWrpGen(string[] args)
        {
            var gen = new RXptrWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            ProcessTestLine(args, gen);
        }

        private static HeaderFilter createFilter()
        {
            var apiFilter = new HeaderFilter();
            apiFilter.ContainsAny = new string[] { "SWIFT_API" };
            apiFilter.ToRemove = new string[] { "SWIFT_API" };
            return apiFilter;
        }
    }
}
