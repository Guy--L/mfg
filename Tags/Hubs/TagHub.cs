using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Microsoft.AspNet.SignalR;
using Tags.Models;

namespace Tags.Hubs
{
    public class TagHub : Hub
    {
        private static Dictionary<int, List<Current>> _cache = null;
        private static ConcurrentDictionary<int, int> _audience = null;
        private static SqlDependencyEx _db;
        
        public static void Start()
        {
            _cache = new Dictionary<int, List<Current>>();
            _audience = new ConcurrentDictionary<int, int>();
            _db = new SqlDependencyEx(
                        ConfigurationManager.ConnectionStrings["tag"].ConnectionString
                        , "taglogs"
                        , "subscription"
                        , listenerType: SqlDependencyEx.NotificationTypes.Update);
            _db.TableChanged += UpdateCache;
            _db.Start();
        }

        public static void Stop()
        {
            _cache.Clear();
            _audience.Clear();
            _db.Stop();
            _db.TableChanged -= UpdateCache;
        }

        private void Listen(int tagid, int minutes)
        {
            if (_audience.Keys.Contains(tagid))
            {
                _audience[tagid]++;
                return;
            }

            var c = Current.SingleOrDefault(tagid);
            if (c == null)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("TagHub.Listen: "+tagid+" not found in Current"));
                return;
            }

            var s = new Subscription()
            {
                TagId = tagid,
                Stamp = c.Stamp,
                Value = c.Value
            };
            s.Save();
            _cache[tagid] = c;
            _audience[tagid] = 1;
        }

        private void Ignore(int tagid)
        {
            if (_audience == null || _cache == null)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("TagHub.Ignore: _audience or _cache null"));
                return;         // unexpected error
            }

            int circulation = 0;
            if (!_audience.TryGetValue(tagid, out circulation))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("TagHub.Ignore: " + tagid + " not found in _count"));
                return;         // unexpected error
            }

            if (circulation > 1)
            {
                _audience[tagid] = circulation - 1;
                return;
            }

            Subscription.Delete(tagid);
            if (_audience.TryRemove(tagid, out circulation))
                _cache.Remove(tagid);
            else
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("TagHub.Ignore: " + tagid + " not removed from _audience"));
        }

        private static void UpdateCache(object sender, SqlDependencyEx.TableChangedEventArgs e)
        {
            Debug.WriteLine(e.Data);
            // parse XML
            // foreach(var tagid in parsedxml)
            //    _cache[tagid] = value;
        }

        public void Subscribe(int tagid)
        {
            Listen(tagid);
            Clients.Client(Context.ConnectionId).
            Groups.Add(Context.ConnectionId, tagid.ToString());
        }

        public void Unsubscribe(int tagid)
        {
            Ignore(tagid);
            Groups.Remove(Context.ConnectionId, tagid.ToString());
        }
    }
}
