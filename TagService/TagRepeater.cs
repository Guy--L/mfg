using System.ServiceProcess;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kepware.ClientAce.OpcDaClient;

[assembly: OwinStartup(typeof(TagService.Startup))]
namespace TagService
{
    public partial class TagRepeater : ServiceBase
    {
        DaServerMgt svr;

        public TagRepeater()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            if (!EventLog.SourceExists("TagRepeater"))
            {
                EventLog.CreateEventSource("TagRepeater", "Application");
            }
            eventLog1.Source = "TagRepeater";
            eventLog1.Log = "Application";
            svr = new DaServerMgt();

            eventLog1.WriteEntry("In OnStart");
            string url = "http://localhost:8080";
            WebApp.Start(url);
            
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop");
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


    public static class UserHandler //this static class is to store the number of users conected at the same time
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
        public static Dictionary<string, string> Map = new Dictionary<string, string>();
    }

    public class TagHub : Hub
    {
        public void Write(string id, object value)
        {
            TagService.Write(id, value);
        }

        public void SetGroup(string group)
        {
            Groups.Add(Context.ConnectionId, group);
            UserHandler.Map[Context.ConnectionId] = group;
            Debug.WriteLine("added cxn to group " + group);
            Clients.Client(Context.ConnectionId).Load(TagService.ReadAll(group));
        }

        public override Task OnConnected() //override OnConnect, OnReconnected and OnDisconnected to know if a user is connected or disconnected
        {
            UserHandler.Map.Add(Context.ConnectionId, "");
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            Debug.WriteLine("reconnected cxn");
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            TagService.UnSubscribe(UserHandler.Map[Context.ConnectionId]);
            Debug.WriteLine("disconnected");
            UserHandler.Map.Remove(Context.ConnectionId);
            return base.OnDisconnected();
        }

    }

}
