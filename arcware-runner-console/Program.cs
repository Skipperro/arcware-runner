using System;
using arcware_runner;


namespace arcware_runner_console
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var si = new arcware_runner.SystemInfo(true);
            //arcware_runner.SystemInfo.WriteInfo();
            
            //Console.Write(si.GetJSON());
            AppsController.I.Reset();

        }
    }
}