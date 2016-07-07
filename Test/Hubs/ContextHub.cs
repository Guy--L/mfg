using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Test.Models;

namespace Test.Hubs
{
    public class ContextHub : Hub
    {
        public static ConcurrentDictionary<string, Context> _contexts = new ConcurrentDictionary<string, Context>();

        public void ByProduct(string product)
        {
            string code = product.Trim().ToUpper();
            string spec = "";

            var space = product.Trim().Replace('\t',' ').IndexOf(' ');
            if (space > -1)
            {
                var split = product.Split(' ');
                code = split[0];
                spec = split[1];
            }
            Context ctx = new Context(code, spec);
            _contexts.AddOrUpdate(Context.ConnectionId, ctx, (k, v) => ctx);
            Clients.Caller.Context(ctx);
        }

        public void ByLotNum(string lotnum)
        {
            Context ctx = new Models.Context(lotnum.Trim().ToUpper());

            _contexts.AddOrUpdate(Context.ConnectionId, ctx, (k, v) => ctx);
            Clients.Caller.Context(ctx);
        }

        public override Task OnConnected()
        {
            _contexts.TryAdd(Context.ConnectionId, null);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            _contexts.TryAdd(Context.ConnectionId, null);
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Context garbage;
            _contexts.TryRemove(Context.ConnectionId, out garbage);

            return base.OnDisconnected(stopCalled);
        }
    }
}