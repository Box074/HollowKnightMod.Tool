using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HKTool.Core;

namespace HKToolCli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) return;
            if (args[0].Equals("MAKE-FAKE", StringComparison.CurrentCultureIgnoreCase))
            {
                FakeLibrary.MakeFakeLibrary(args[1],
                    Path.Combine(
                        Path.GetDirectoryName(Path.GetFullPath(args[1])),
                        Path.GetFileNameWithoutExtension(args[1]) + ".fake" + Path.GetExtension(args[1])
                        )
                        );

            }
            else if(args[0].Equals("FAKE-PARSE", StringComparison.CurrentCultureIgnoreCase))
            {
                ParseFakeLibrary.ParseAssembly(args[1],
                    Path.Combine(
                        Path.GetDirectoryName(Path.GetFullPath(args[1])),
                        Path.GetFileNameWithoutExtension(args[1]) + ".fake" + Path.GetExtension(args[1])
                        )
                        );
            }
            else
            {
                
            }
        }
    }
}
