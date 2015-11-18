using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            A t2 = new A();
            A t1 = test();
            Console.WriteLine(Encrypt(t1));
            Console.WriteLine(Encrypt(t2));
            Console.ReadLine();
        }

        public static A test()
        {
            A tt = new B();
            return tt;
        }
        public static int Encrypt(A m)
        {
            int type = -1;
            if (m is A)
            {
                type = 0;
            }
            if (m is B)
            {
                type = 1;
            }
            return type;
        }
    }

        public class A
        {

        }

        public class B : A
        {

        }
    
}
