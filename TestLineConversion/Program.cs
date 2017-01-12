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
            //TestStrings();

            TestWrpGen(args);
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

        private static void TestWrpGen(string[] args)
        {
            HeaderFilter filter = createFilter();
            string apiLine = args[0];
            var gen = new CsharpApiWrapperGenerator();
            gen.AddCustomWrapper(gen.ReturnsCharPtrPtrWrapper());
            apiLine = filter.FilterInput(apiLine)[0];
            var w = new WrapperGenerator(gen, filter);
            var result = w.Convert(new string[] { apiLine });
            Console.WriteLine(result[0]);
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
