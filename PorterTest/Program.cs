using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace PorterTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = SerialPort.GetPortNames();
            Console.WriteLine(string.Join("\n", list));
            var port = new SerialPort();
            port.PortName = "COM5";
            port.Open();
            try
            {
                while (true)
                {
                    port.WriteLine(((double)DateTime.Now.Second/10.0).ToString());
                    Thread.Sleep(1000);
                }
            }
            finally
            {
                port.Close();
            }
        }
    }
}
