using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Key.Models
{
    public partial class User
    {
        private static string _log = @"
            select c.Name, u.[Identity], o.Notes, o.Stamp 
            from [Operation] o
            join [HMI] h on o.HMIId = h.HMIId
            join [Channel] c on c.ChannelId = h.ChannelId
            join [User] u on u.UserId = o.UserId  
        ";
        private static string _getid = @"
            merge into [user] as t
            using (select [userid] from [user] where [identity] = @0) as s
            on t.userid = s.userid
            when not matched then
	            insert ([identity],[login]) values (@0, '');
            select userid from [user] where [identity] = @0
        ";

        public User() { }

        public User(string id)
        {
            using (keyDB k = new keyDB())
            {
                UserId = k.ExecuteScalar<int>(_getid, id);
            }
        }
    }
}