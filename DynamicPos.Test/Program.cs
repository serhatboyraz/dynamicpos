using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPos.WebServer.Helper;

namespace DynamicPos.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerHelper.GetInstance().Start();
            Console.ReadLine();
        }
    }
}