using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public class Picture
    {
        public static string initCurrent = @"
            insert into [Current] (TagId, Name)
            select b.TagId, b.[Name]+'.'+d.[Name]+'.'+t.[Name] as [Name]
            from [Current] a
            right join Tag b on a.TagId = b.TagId
            join Device d on b.DeviceId = d.DeviceId
            join Channel c on d.ChannelId = c.ChannelId
            where a.TagId is null
        ";
        public static string applyIdsToLogs = @"
            with tagu as (
	            select c.[Name]+'.'+d.[Name]+'.'+t.[Name] as [path], t.TagId 
	            from [Tag] t 
	            join Device d on t.DeviceId = d.DeviceId
	            join Channel c on d.ChannelId = c.ChannelId
            )
            UPDATE [All]
            SET TagId = tagu.TagId
            FROM tagu
            where [All].Name = tagu.[path] and TagId = 0
        ";
        public static string duplicates = @"
            select b.Pair, tt.Name, cc.Name, dd.Name, tt.[Address]
            from Tag tt
            join Device dd on dd.DeviceId = tt.DeviceId
            join Channel cc on cc.ChannelId = dd.ChannelId
            join (SELECT ROW_NUMBER() over (order by t.[Address]) as Pair, c.[ChannelId] as Channel, d.[DeviceId] as Device, t.[Address] as [Address]
                FROM [Tag] t
	            join [Device] d on d.DeviceId = t.DeviceId
	            join [Channel] c on c.ChannelId = d.ChannelId
            GROUP BY   c.[ChannelId], d.[DeviceId], t.[Address]
            HAVING COUNT(*) > 1) b 
            on  b.Channel = cc.ChannelId
            and b.Device  = dd.DeviceId
            and b.[Address] = tt.[Address]
            order by b.[Pair]
        ";
        public static string resetdb = @"
            delete from tag
            delete from device
            delete from channel
            delete from [current]
            DBCC CHECKIDENT ('[tag]', RESEED, 0)
            DBCC CHECKIDENT ('[device]', RESEED, 0)
            DBCC CHECKIDENT ('[channel]', RESEED, 0)
        ";
        public static string loggedWithoutId = @"
            select distinct Name from [All] where not exists (
            select 'x' from [Tag] t
            join Device d on t.DeviceId = d.DeviceId
            join Channel c on d.ChannelId = c.ChannelId
            where c.[Name]+'.'+d.[Name]+'.'+t.[Name] = [All].Name
            ) 
            ";
        public static string current = @"
            select a.Name, a.Value, a.Stamp, a.TagId
            from [All] as a inner join (
            select Name, max(Stamp) as MaxStamp
            from [All] group by Name)
            as b on a.Name = b.Name and a.Stamp = b.MaxStamp 
            ";
    }
}