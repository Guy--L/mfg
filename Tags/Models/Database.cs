
// This file was automatically generated by the PetaPoco T4 Template
// Do not make changes directly to this file - edit the template instead
// 
// The following connection settings were used to generate this file
// 
//     Connection String Name: `tag`
//     Provider:               `System.Data.SqlClient`
//     Connection String:      `Data Source=GUYLISTER3546;Initial Catalog=taglogs;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False`
//     Schema:                 `dbo`
//     Include Views:          `True`

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Tags.Models
{
	public partial class tagDB : Database
	{
		public tagDB() 
			: base("tag")
		{
			CommonConstruct();
		}

		public tagDB(string connectionStringName) 
			: base(connectionStringName)
		{
			CommonConstruct();
		}
		
		partial void CommonConstruct();
		
		public interface IFactory
		{
			tagDB GetInstance();
		}
		
		public static IFactory Factory { get; set; }
        public static tagDB GetInstance()
        {
			if (_instance!=null)
				return _instance;
				
			if (Factory!=null)
				return Factory.GetInstance();
			else
				return new tagDB();
        }

		[ThreadStatic] static tagDB _instance;
		
        protected override void OnBeginTransaction()
        {
                if (_instance==null)
                        _instance=this;
        }
                
        protected override void OnCompleteTransaction()
        {
                if (_instance==this)
                        _instance=null;
        }
				        
		public class Record<T> where T:new()
		{
			public static tagDB repo { get { return tagDB.GetInstance(); } }
			public bool IsNew() { return repo.IsNew<T>(this); }
			public object Insert() { return repo.Insert(this); }
			public void Save() { repo.Save<T>(this); }
			public int Update() { return repo.Update(this); }
			public int Update(IEnumerable<string> columns) { return repo.Update(this, columns); }
			public static int Update(string sql, params object[] args) { return repo.Update<T>(sql, args); }
			public static int Update(Sql sql) { return repo.Update<T>(sql); }
			public int Delete() { return repo.Delete(this); }
			public static int Delete(string sql, params object[] args) { return repo.Delete<T>(sql, args); }
			public static int Delete(Sql sql) { return repo.Delete<T>(sql); }
			public static int Delete(object primaryKey) { return repo.Delete<T>(primaryKey); }
			public static bool Exists(object primaryKey) { return repo.Exists<T>(primaryKey); }
			public static T SingleOrDefault(object primaryKey) { return repo.SingleOrDefaultById<T>(primaryKey); }
			public static T SingleOrDefault(string sql, params object[] args) { return repo.SingleOrDefault<T>(sql, args); }
			public static T SingleOrDefault(Sql sql) { return repo.SingleOrDefault<T>(sql); }
			public static T FirstOrDefault(string sql, params object[] args) { return repo.FirstOrDefault<T>(sql, args); }
			public static T FirstOrDefault(Sql sql) { return repo.FirstOrDefault<T>(sql); }
			public static T Single(object primaryKey) { return repo.SingleById<T>(primaryKey); }
			public static T Single(string sql, params object[] args) { return repo.Single<T>(sql, args); }
			public static T Single(Sql sql) { return repo.Single<T>(sql); }
			public static T First(string sql, params object[] args) { return repo.First<T>(sql, args); }
			public static T First(Sql sql) { return repo.First<T>(sql); }
			public static List<T> Fetch(string sql, params object[] args) { return repo.Fetch<T>(sql, args); }
			public static List<T> Fetch(Sql sql) { return repo.Fetch<T>(sql); }
			public static List<T> Fetch(long page, long itemsPerPage, string sql, params object[] args) { return repo.Fetch<T>(page, itemsPerPage, sql, args); }
			public static List<T> Fetch(long page, long itemsPerPage, Sql sql) { return repo.Fetch<T>(page, itemsPerPage, sql); }
			public static List<T> SkipTake(long skip, long take, string sql, params object[] args) { return repo.SkipTake<T>(skip, take, sql, args); }
			public static List<T> SkipTake(long skip, long take, Sql sql) { return repo.SkipTake<T>(skip, take, sql); }
			public static Page<T> Page(long page, long itemsPerPage, string sql, params object[] args) { return repo.Page<T>(page, itemsPerPage, sql, args); }
			public static Page<T> Page(long page, long itemsPerPage, Sql sql) { return repo.Page<T>(page, itemsPerPage, sql); }
			public static IEnumerable<T> Query(string sql, params object[] args) { return repo.Query<T>(sql, args); }
			public static IEnumerable<T> Query(Sql sql) { return repo.Query<T>(sql); }
		}
	}
	

	[TableName("User")]
	[PrimaryKey("UserId")]
	[ExplicitColumns]
    public partial class User : tagDB.Record<User>  
    {		
		[Column] public int UserId { get; set; } 		
		[Column] public string Identity { get; set; } 		
		[Column] public string Login { get; set; } 	
	}

	[TableName("Graph")]
	[PrimaryKey("GraphId")]
	[ExplicitColumns]
    public partial class Graph : tagDB.Record<Graph>  
    {		
		[Column] public int GraphId { get; set; } 		
		[Column] public string GraphName { get; set; } 		
		[Column] public int OwnerId { get; set; } 	
	}

	[TableName("Plot")]
	[PrimaryKey("PlotId")]
	[ExplicitColumns]
    public partial class Plot : tagDB.Record<Plot>  
    {		
		[Column] public int PlotId { get; set; } 		
		[Column] public int GraphId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public bool YAxis { get; set; } 		
		[Column] public string Relabel { get; set; } 		
		[Column] public int Scale { get; set; } 		
		[Column] public int MinY { get; set; } 		
		[Column] public int MaxY { get; set; } 	
	}

	[TableName("SubMinute")]
	[ExplicitColumns]
    public partial class SubMinute : tagDB.Record<SubMinute>  
    {		
		[Column] public int TagId { get; set; } 		
		[Column("SubMinute")] public int _SubMinute { get; set; }
	
	}

	[TableName("sample")]
	[ExplicitColumns]
    public partial class sample : tagDB.Record<sample>  
    {		
		[Column] public int AllId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public string Value { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int Quality { get; set; } 	
	}

	[TableName("HMI")]
	[PrimaryKey("HMIId")]
	[ExplicitColumns]
    public partial class HMI : tagDB.Record<HMI>  
    {		
		[Column] public int HMIId { get; set; } 		
		[Column] public int ChannelId { get; set; } 		
		[Column] public int ChartId { get; set; } 		
		[Column] public int RequestPending { get; set; } 		
		[Column] public bool RequestComplete { get; set; } 		
		[Column] public bool Error { get; set; } 		
		[Column] public DateTime Expires { get; set; } 	
	}

	[TableName("Past")]
	[PrimaryKey("PastId", AutoIncrement=false)]
	[ExplicitColumns]
    public partial class Past : tagDB.Record<Past>  
    {		
		[Column] public int PastId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public string Value { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 	
	}

	[TableName("UserGraph")]
	[PrimaryKey("UserGraphId")]
	[ExplicitColumns]
    public partial class UserGraph : tagDB.Record<UserGraph>  
    {		
		[Column] public int UserGraphId { get; set; } 		
		[Column] public int UserId { get; set; } 		
		[Column] public int GraphId { get; set; } 		
		[Column] public bool Shared { get; set; } 	
	}

	[TableName("Channel")]
	[PrimaryKey("ChannelId")]
	[ExplicitColumns]
    public partial class Channel : tagDB.Record<Channel>  
    {		
		[Column] public int ChannelId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public int? PlantId { get; set; } 	
	}

	[TableName("Device")]
	[PrimaryKey("DeviceId")]
	[ExplicitColumns]
    public partial class Device : tagDB.Record<Device>  
    {		
		[Column] public int DeviceId { get; set; } 		
		[Column] public int ChannelId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public string IPAddress { get; set; } 		
		[Column] public string Model { get; set; } 	
	}

	[TableName("Operation")]
	[PrimaryKey("OperationId")]
	[ExplicitColumns]
    public partial class Operation : tagDB.Record<Operation>  
    {		
		[Column] public int OperationId { get; set; } 		
		[Column] public int HMIId { get; set; } 		
		[Column] public int UserId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int ApproverId { get; set; } 		
		[Column] public string Notes { get; set; } 	
	}

	[TableName("Role")]
	[PrimaryKey("RoleId")]
	[ExplicitColumns]
    public partial class Role : tagDB.Record<Role>  
    {		
		[Column] public int RoleId { get; set; } 		
		[Column("Role")] public string _Role { get; set; }
		
		[Column] public string ADGroup { get; set; } 	
	}

	[TableName("Recent")]
	[PrimaryKey("RecentId")]
	[ExplicitColumns]
    public partial class Recent : tagDB.Record<Recent>  
    {		
		[Column] public int RecentId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public string Value { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int Quality { get; set; } 	
	}

	[TableName("UserRole")]
	[PrimaryKey("UserRoleId")]
	[ExplicitColumns]
    public partial class UserRole : tagDB.Record<UserRole>  
    {		
		[Column] public int UserRoleId { get; set; } 		
		[Column] public int UserId { get; set; } 		
		[Column] public int RoleId { get; set; } 	
	}

	[TableName("Tag")]
	[PrimaryKey("TagId")]
	[ExplicitColumns]
    public partial class Tag : tagDB.Record<Tag>  
    {		
		[Column] public int TagId { get; set; } 		
		[Column] public int DeviceId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public string Address { get; set; } 		
		[Column] public string DataType { get; set; } 		
		[Column] public bool IsLogged { get; set; } 		
		[Column] public bool IsArchived { get; set; } 		
		[Column] public int? RelatedTagId { get; set; } 	
	}

	[TableName("TagHistory")]
	[PrimaryKey("TagHistoryId")]
	[ExplicitColumns]
    public partial class TagHistory : tagDB.Record<TagHistory>  
    {		
		[Column] public int TagHistoryId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public bool Started { get; set; } 		
		[Column] public int Seconds { get; set; } 		
		[Column] public int Type { get; set; } 	
	}

	[TableName("Group")]
	[PrimaryKey("GroupId")]
	[ExplicitColumns]
    public partial class Group : tagDB.Record<Group>  
    {		
		[Column] public int GroupId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public int UserId { get; set; } 	
	}

	[TableName("TagGroup")]
	[PrimaryKey("TagGroupId")]
	[ExplicitColumns]
    public partial class TagGroup : tagDB.Record<TagGroup>  
    {		
		[Column] public int TagGroupId { get; set; } 		
		[Column] public int GroupId { get; set; } 		
		[Column] public int TagId { get; set; } 	
	}

	[TableName("CurrentValues")]
	[ExplicitColumns]
    public partial class CurrentValue : tagDB.Record<CurrentValue>  
    {		
		[Column] public string Name { get; set; } 		
		[Column] public string Value { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int TagId { get; set; } 	
	}

	[TableName("Control")]
	[ExplicitColumns]
    public partial class Control : tagDB.Record<Control>  
    {		
		[Column] public int ControlId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public string StringValue { get; set; } 		
		[Column] public int? BCDValue { get; set; } 		
		[Column] public double? FloatValue { get; set; } 		
		[Column] public bool? BooleanValue { get; set; } 		
		[Column] public int? WordValue { get; set; } 	
	}

	[TableName("Limit")]
	[PrimaryKey("LimitId")]
	[ExplicitColumns]
    public partial class Limit : tagDB.Record<Limit>  
    {		
		[Column] public int LimitId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public double LoLo { get; set; } 		
		[Column] public double Lo { get; set; } 		
		[Column] public double Aim { get; set; } 		
		[Column] public double Hi { get; set; } 		
		[Column] public double HiHi { get; set; } 	
	}

	[TableName("Current")]
	[PrimaryKey("TagId", AutoIncrement=false)]
	[ExplicitColumns]
    public partial class Current : tagDB.Record<Current>  
    {		
		[Column] public int TagId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public string Value { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int SubMinute { get; set; } 	
	}

	[TableName("All")]
	[PrimaryKey("AllId")]
	[ExplicitColumns]
    public partial class All : tagDB.Record<All>  
    {		
		[Column] public int AllId { get; set; } 		
		[Column] public int TagId { get; set; } 		
		[Column] public string Value { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int Quality { get; set; } 	
	}

}



