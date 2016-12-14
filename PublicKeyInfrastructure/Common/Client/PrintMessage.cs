using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Client
{
    public class PrintMessage
    {
        public static void Print(string msg)
        {
            Console.WriteLine();
            Console.WriteLine("/////////////////////////////////////////////");
            Console.WriteLine("     {0}", msg);
            Console.WriteLine("/////////////////////////////////////////////");
            Console.WriteLine();
        }
    }
}
