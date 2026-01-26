using System;

namespace DoraExplorer.DotNetTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("What's your name?");
            var name = Console.ReadLine();
            Console.WriteLine("How old are you?");
            var age = Console.ReadLine();
            var response = DoraExplorer.Core.Class1.Greet(name, int.Parse(age));
            Console.WriteLine(response);
        }
    }
}