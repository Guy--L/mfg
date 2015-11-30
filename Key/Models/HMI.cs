using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Key.Models
{
    public partial class HMI
    {
        public static string _all = @"
            select h.HMIId, h.ChannelId, h.ChartId, h.RequestPending, h.RequestComplete, h.Error, c.Name as ChannelName
            from HMI h
            join Channel c on h.ChannelId = c.ChannelId
        ";
        private static string _unlock = @"
            update HMI 
	            set RequestPending = 2, Expires = dateadd(minute, 5, getdate())
	            where HMIId = @0 and RequestPending = 1 and [Error] = 0
            insert operation 
				(userid, HMIId, Stamp, notes, approverid)
				values
                (@1, @0, getdate(), 'unlocked', 0)
            select @@@rowcount
        ";
        private static string _lock = @"
            update HMI 
	            set RequestPending = 1
	            where HMIId = @0 and RequestPending = 2
            insert operation 
				(userid, HMIId, Stamp, notes, approverid)
				values
                (@1, @0, getdate(), 'locked', 0)
            select @@@rowcount
        ";        
        [ResultColumn] public string ChannelName { get; set; }
        public bool IsLocked { get { return RequestPending != 2; } }
        public string message { get; set; }

        public HMI()
        { }

        public HMI(int id, int user)
        {
            var aid = id < 0? -id: id;
            var qry = id < 0? _lock: _unlock;

            int r = 0;
            using (keyDB k = new keyDB())
            {
                r = k.ExecuteScalar<int>(qry, aid, user);
            }
            HMI h = SingleOrDefault(_all + " where HMIId = @0", aid);
            ChannelId = h.ChannelId;
            HMIId = h.HMIId;	
            ChartId = h.ChartId;	
            ChannelName = h.ChannelName;
            RequestPending = h.RequestPending;
            RequestComplete = h.RequestComplete;
            Error = h.Error;
            message = ChannelName + " was " + (r == 0 ? "not " : " ") + (id < 0 ? "locked" : "unlocked");
        }

        public static List<HMI> Locks()
        {
            List<HMI> hmis = null;
            using (keyDB k = new keyDB()) 
            {
                hmis = k.Fetch<HMI>(_all).OrderBy(m => m.ChannelName).ToList();
            }
            return hmis;
        }
    }

    public class HMIView
    {
        public List<HMI> hmis { get; set; }
        public string Name { get; set; }
        public int requested { get; set; }

        public HMIView() { }

        public HMIView(bool b)
        {
            hmis = HMI.Locks();
        }
    }
}