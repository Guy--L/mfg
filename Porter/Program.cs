using System;
using System.IO.Ports;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Collections.Generic;
using System.Management;
using System.Linq;

namespace Porter
{
    internal class ProcessConnection
    {

        public static ConnectionOptions ProcessConnectionOptions()
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.Authentication = AuthenticationLevel.Default;
            options.EnablePrivileges = true;
            return options;
        }

        public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)
        {
            ManagementScope connectScope = new ManagementScope();
            connectScope.Path = new ManagementPath(@"\\" + machineName + path);
            connectScope.Options = options;
            connectScope.Connect();
            return connectScope;
        }
    }

    public class COMPortInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public COMPortInfo() { }

        public static List<COMPortInfo> GetCOMPortsInfo()
        {
            List<COMPortInfo> comPortInfoList = new List<COMPortInfo>();

            ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
            ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");

            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

            using (comPortSearcher)
            {
                string caption = null;
                foreach (ManagementObject obj in comPortSearcher.Get())
                {
                    if (obj != null)
                    {
                        object captionObj = obj["Caption"];
                        if (captionObj != null)
                        {
                            caption = captionObj.ToString();
                            if (caption.Contains("(COM"))
                            {
                                COMPortInfo comPortInfo = new COMPortInfo();
                                comPortInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")",
                                                                     string.Empty);
                                comPortInfo.Description = caption;
                                comPortInfoList.Add(comPortInfo);
                            }
                        }
                    }
                }
            }
            return comPortInfoList;
        }
    }

    class Program
    {
        static private List<SerialPort> _ports = null;
        static IHubContext _context = null;
        static List<COMPortInfo> portlist = null;

        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            _ports = new List<SerialPort>();
            string url = "http://localhost:8080";

            portlist = COMPortInfo.GetCOMPortsInfo();
            foreach (COMPortInfo comPort in portlist)
            {
                Console.WriteLine(string.Format("{0} – {1}", comPort.Name, comPort.Description));
            }

            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.WriteLine("Press 'X' to close...");
                Console.WriteLine();

                ConsoleKeyInfo k;
                do
                {
                    k = Console.ReadKey();
                } while (k.KeyChar != 'x' || k.KeyChar != 'X');

                foreach (var p in _ports)
                    p.Close();
            }
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            Console.WriteLine(indata);
            HubContext.Clients.All.updateField(indata);
        }

        public static IHubContext HubContext
        {
            get
            {
                if (_context == null)
                    _context = GlobalHost.ConnectionManager.GetHubContext<PortHub>();

                return _context;
            }
        }

        public static void Rqst(int port, string command)
        {
            _ports[port].WriteLine(command);
        }

        public static string List()
        {
            var ports = SerialPort.GetPortNames();
            return string.Join("\n", ports);
        }

        public static int Add(string portname)
        {
            var port = portlist.Where(p => p.Description.StartsWith(portname)).Select(p => p.Name).SingleOrDefault();
            if (port == null)
            {
                Console.WriteLine("Could not find port for " + portname);
                return -1;
            }
            var s = new SerialPort(port);
            s.DataReceived += Port_DataReceived;
            _ports.Add(s);
            Console.Write(portname + " connected to " + portname);
            s.Open();
            var i = _ports.IndexOf(s);
            Console.WriteLine(" as port " + i);
            return i;
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
    public class PortHub : Hub
    {
        static Dictionary<int, string> commands;

        public void List()
        {
            var ports = Program.List();
            Clients.All.list(ports);
        }

        public void Add(string port, string command)
        {
            if (commands == null)
                commands = new Dictionary<int, string>();

            var index = Program.Add(port);
            if (index >= 0) commands[index] = command;
        }

        public void Request(int port)
        {
            var command = commands[port];
            Console.WriteLine("request "+command+" on port " + port);
            Program.Rqst(port, command);
        }

        public void Response(string message)
        {
            Clients.All.updateField(message);
        }
    }
}
