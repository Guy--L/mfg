using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Test.Hubs
{
    public class RefreshHub : Hub
    {
        public void Register(string page)
        {
            Groups.Add(Context.ConnectionId, page);
        }

        public void Refresh(string page)
        {
            Clients.Group(page).refresh();
        }

        public void UpVersion(string page)
        {
            Clients.Group(page).upVersion();
        }
    }
}