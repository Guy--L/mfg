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
        public static ConcurrentDictionary<string, Context> contexts = new ConcurrentDictionary<string, Context>();

        public List<Run> RunsEver()
        {
            Context _top = contexts[Context.User.Identity.Name];

            var runs = Run.RunsEver(_top.ProductCodeId);

            if (_top.SampleId != 0)
                runs.Add(new Run(_top));                        // add lot context if needed

            return runs;
        }

        public List<TagSample> RunDetail(string channel, long start, long end)
        {
            var samples = TagSample.Span(channel, start.FromJSMSecs(), end.FromJSMSecs());

            return samples;
        }

        public Context ByProduct(string product)
        {
            string code = product.Trim().ToUpper();
            string spec = null;

            var space = product.Trim().Replace('\t',' ').IndexOf(' ');
            if (space > -1)
            {
                var split = product.Split(' ');
                code = split[0];
                spec = split[1];
                if (split.Length == 3 && spec == "I")
                    spec = "I " + split[2];
                else if (split[1][0] == 'I' && split.Length == 2)
                    spec = "I " + split[1].Substring(1);
            }
            Context ctx = new Context(code, spec);
            ctx.ConnectionId = Context.ConnectionId;

            contexts.AddOrUpdate(Context.User.Identity.Name, ctx, (k, v) => ctx);
            return ctx;
        }

        public Context ByLotNum(string lotnum)
        {
            Context ctx = new Context(lotnum.Trim().ToUpper());
            ctx.ConnectionId = Context.ConnectionId;

            contexts.AddOrUpdate(Context.User.Identity.Name, ctx, (k, v) => ctx);
            return ctx;
        }

        public Context Clear()
        {
            Context ctx;
            contexts.TryRemove(Context.User.Identity.Name, out ctx);
            ctx = new Context();
            ctx.ConnectionId = Context.ConnectionId;
            return ctx;
        }

        public Context ClearSample()
        {
            Context ctx = null;
            contexts.TryGetValue(Context.User.Identity.Name, out ctx);
            if (ctx == null)
                return null;

            ctx.ConnectionId = Context.ConnectionId;
            ctx.LotNum = "";
            ctx.SampleId = 0;
            contexts.AddOrUpdate(Context.User.Identity.Name, ctx, (k, v) => ctx);
            return ctx;
        }

        public List<int> Stats()
        {
            Context ctx = null;
            contexts.TryGetValue(Context.User.Identity.Name, out ctx);
            if (ctx == null)
                return null;
            return ctx.Stats();
        }

        public override Task OnConnected()
        {
            contexts.TryAdd(Context.User.Identity.Name, null);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            contexts.TryAdd(Context.User.Identity.Name, null);
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Context garbage;
            contexts.TryRemove(Context.User.Identity.Name, out garbage);

            return base.OnDisconnected(stopCalled);
        }
    }
}