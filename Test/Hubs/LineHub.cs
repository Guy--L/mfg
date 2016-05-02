using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Test.Models;

namespace Test.Hubs
{
    public class LineHub : Hub
    {
        private static Dictionary<string, Dictionary<int, int>> sampleids = new Dictionary<string, Dictionary<int, int>>();
        private static Dictionary<int, LineOperation> status = new Dictionary<int, LineOperation>();
        private static Dictionary<int, LineOperation> lastStatus = new Dictionary<int, LineOperation>();
        private static Dictionary<int, ProductCode> specification = new Dictionary<int, ProductCode>();

        public void SetLineTime(DateTime moment)
        {
            using (labDB db = new labDB())
            {
                var systems = db.Fetch<Models.System>(string.Format(Models.System._attime, moment.ToStamp()));
                Clients.Client(Context.ConnectionId).setSystems(systems.Select(s => new { id = s.SystemId, value = s._System + " " + s.SolutionType }));
            }
        }

        // following methods support casingsample.cshtml

        void SetSampleTime(DateTime oldTime, DateTime newTime)
        {
            var ol = oldTime.ToString("ddhhmmtt");
            var nw = newTime.ToString("ddhhmmtt");

            if (ol != nw) Groups.Remove(Context.ConnectionId, ol);
            Groups.Add(Context.ConnectionId, nw);
            if (!sampleids.ContainsKey(nw))
                sampleids[nw] = new Dictionary<int, int>();
        }

        
        /// <summary>
        /// Update fields in database one entry at a time
        /// </summary>
        /// <param name="id">Id of the sample</param>
        /// <param name="rid">Id of the reading</param>
        /// <param name="set">Scheduled time of the sample</param>
        /// <param name="lineid">Mfg line number</param>
        /// <param name="field">Web page element id which contains name of field</param>
        /// <param name="value">value to put in database</param>
        public void UpdateCasingSample(int id, int rid, DateTime set, int lineid, string field, object value, string tech, int product)
        {
            var element = field;
            var render = field.Split('_');
            var isGly = render.Contains("Gly");
            var isOil = render.Contains("Oil");
            var group = set.ToString("ddhhmmtt");
            field = (isGly || isOil) ? string.Join("_", render.Skip(5).Take(2).ToArray()) : render.Last();

            int sid = -1;
            if (id == 0 && sampleids[group].TryGetValue(lineid, out sid))
            {
                id = sid;
            }

            CasingSample cs = new CasingSample(id);

            cs.Tech = tech;
            cs.ProductCodeId = product;
            cs.LineId = lineid;
            if (set != null) cs.Scheduled = set;

            Reading r = null;
            if (isGly || isOil)
            {
                r = new Reading(rid, cs.ParameterId);
                r.SampleId = cs.SampleId;
                r.Scheduled = cs.Scheduled;
                r.Stamp = DateTime.Now;
                r.LineId = cs.LineId;
                r.Operator = tech;
                r.EditCount = 0;

                if (isGly)
                    cs.Gly = r;
                else
                    cs.Oil = r;
            }

            string note = value as string;
            Int32? reading = null;

            try { reading = Convert.ToInt32(value); } catch { }

            object val = note == null ? (object)reading : (object)note;

            var property = CasingSample.reflect[field];
            property.SetValue(cs, val);

            if (!(isGly || isOil) || id == 0)
            {
                cs.Save();
                if (id == 0)
                    sampleids[group].Add(lineid, cs.SampleId);
            }

            int newid = rid;
            if (isGly || isOil)
            {
                r.Save();
                newid = r.ReadingId;
            }

            // reflect values to all clients including the submitter because 
            // new records may now have been created and 0 needs to be replaced on the submitters page

            if (note != null)
                Clients.Group(group).reflectNote(element, cs.SampleId, newid, val);
            else
                Clients.Group(group).reflectField(element, cs.SampleId, newid, val);
        }

        public DateTime NextSample(int line, int type)
        {
            return Sample.NextSample(line, type);
        }

        public List<Sample> NextSamples(int line)
        {
            return Sample.NextSamples(line);
        }
    }
}