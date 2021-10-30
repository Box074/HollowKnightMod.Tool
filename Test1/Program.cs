using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            object Test = 0;
            Type a = Test.GetType();
            object c = a.FullName.ToCharArray().Length + 10;
            Console.WriteLine(c);
            new Test.fake_T("A", "B", "C").fake_TestA();
            Console.ReadLine();
        }
    }
}
