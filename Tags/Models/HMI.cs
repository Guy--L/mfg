using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Tags.Models
{
    public partial class HMI
    {
        public static string _controls = @"
            select h.HMIId
	            , h.ChannelId
	            , h.ChartId
	            , h.RequestPending
	            , h.RequestComplete
	            , h.[Error]
	            , c.[Name] as ChannelName
            from HMI h
            join Channel c on c.ChannelId = h.ChannelId  
        ";

        public static string _sync = @"
			alter table [dbo].[HMI] nocheck constraint all
            merge [dbo].[HMI] h
            using (
                select q.ChannelId
                from Channel q
                where exists 
                (select 1 from Tag t
                join Device d on t.DeviceId = d.DeviceId
                where d.ChannelId = q.ChannelId and t.Name = 'MES_timing')
            ) c 
            on c.ChannelId = h.ChannelId
            when not matched then
                insert (ChannelId, ChartId, RequestPending, RequestComplete, [Error], Expires)
                values (c.ChannelId, 0, 1, 0, 0, dateadd(month, 3, getdate()));
			alter table [dbo].[HMI] check constraint all
        ";

        [ResultColumn] public string ChannelName { get; set; }
    }
}