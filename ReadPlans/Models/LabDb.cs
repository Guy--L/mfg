
// This file was automatically generated by the PetaPoco T4 Template
// Do not make changes directly to this file - edit the template instead
// 
// The following connection settings were used to generate this file
// 
//     Connection String Name: `lab`
//     Provider:               `System.Data.SqlClient`
//     Connection String:      `Data Source=GUYLISTER3546;Initial Catalog=mesdb;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False`
//     Schema:                 `dbo`
//     Include Views:          `True`

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace ReadPlans.Models
{
	public partial class labDB : Database
	{
		public labDB() 
			: base("lab")
		{
			CommonConstruct();
		}

		public labDB(string connectionStringName) 
			: base(connectionStringName)
		{
			CommonConstruct();
		}
		
		partial void CommonConstruct();
		
		public interface IFactory
		{
			labDB GetInstance();
		}
		
		public static IFactory Factory { get; set; }
        public static labDB GetInstance()
        {
			if (_instance!=null)
				return _instance;
				
			if (Factory!=null)
				return Factory.GetInstance();
			else
				return new labDB();
        }

		[ThreadStatic] static labDB _instance;
		
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
			public static labDB repo { get { return labDB.GetInstance(); } }
			public bool IsNew() { return repo.IsNew(this); }
			public object Insert() { return repo.Insert(this); }
			public void Save() { repo.Save(this); }
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
	

	[TableName("Plan")]
	[PrimaryKey("PlanId")]
	[ExplicitColumns]
    public partial class Plan : labDB.Record<Plan>  
    {		
		[Column] public int PlanId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int? ProductCodeId { get; set; } 		
		[Column] public string Code { get; set; } 		
		[Column] public string Spec { get; set; } 		
		[Column] public int ExtruderId { get; set; } 		
		[Column] public string Solution { get; set; } 		
		[Column] public int? SystemId { get; set; } 		
		[Column] public int? SolutionRecipeId { get; set; } 		
		[Column] public string ConversionStatus { get; set; } 		
		[Column] public string Comment { get; set; } 		
		[Column] public int? FinishFootage { get; set; } 	
	}

	[TableName("ProductCode")]
	[PrimaryKey("ProductCodeId")]
	[ExplicitColumns]
    public partial class ProductCode : labDB.Record<ProductCode>  
    {		
		[Column] public int ProductCodeId { get; set; } 		
		[Column("ProductCode")] public string _ProductCode { get; set; }
		
		[Column] public string ProductSpec { get; set; } 		
		[Column] public bool? IsActive { get; set; } 		
		[Column] public string PlastSpec { get; set; } 		
		[Column] public double? WetLayflat_Aim { get; set; } 		
		[Column] public double? WetLayflat_Min { get; set; } 		
		[Column] public double? WetLayflat_Max { get; set; } 		
		[Column] public double? Glut_Aim { get; set; } 		
		[Column] public double? Glut_Max { get; set; } 		
		[Column] public double? Glut_Min { get; set; } 		
		[Column] public double? FFW_Aim { get; set; } 		
		[Column] public double? FFW_Min { get; set; } 		
		[Column] public double? FFW_Max { get; set; } 		
		[Column] public double? CasingWt_Aim { get; set; } 		
		[Column] public double? CasingWt_Min { get; set; } 		
		[Column] public double? CasingWt_Max { get; set; } 		
		[Column] public double? ShirrMoist_Aim { get; set; } 		
		[Column] public double? ShirrMoist_Min { get; set; } 		
		[Column] public double? ShirrMoist_Max { get; set; } 		
		[Column] public double? ReelMoist_Aim { get; set; } 		
		[Column] public double? ReelMoist_Min { get; set; } 		
		[Column] public double? ReelMoist_Max { get; set; } 		
		[Column] public double? LFShirr_Aim { get; set; } 		
		[Column] public double? LFShirr_Min { get; set; } 		
		[Column] public double? LFShirr_Max { get; set; } 		
		[Column] public double? LFShirr_LCL { get; set; } 		
		[Column] public double? LFShirr_UCL { get; set; } 		
		[Column] public double? LF_Aim { get; set; } 		
		[Column] public float? LF_Min { get; set; } 		
		[Column] public float? LF_Max { get; set; } 		
		[Column] public float? LF_LCL { get; set; } 		
		[Column] public float? LF_UCL { get; set; } 		
		[Column] public string OilType { get; set; } 		
		[Column] public double? Oil_Aim { get; set; } 		
		[Column] public double? Oil_Min { get; set; } 		
		[Column] public double? Oil_Max { get; set; } 		
		[Column] public double? Gly_Aim { get; set; } 		
		[Column] public double? Gly_Min { get; set; } 		
		[Column] public double? Gly_Max { get; set; } 		
		[Column] public double? DryTensShirr_Min { get; set; } 		
		[Column] public string Unshirr_Max { get; set; } 		
		[Column] public string Unshirr_Avg { get; set; } 		
		[Column] public string WetTens_Min { get; set; } 		
		[Column] public string BlowShirr_Aim { get; set; } 		
		[Column] public string BlowShirr_Min { get; set; } 		
		[Column] public string BlowShirr_Max { get; set; } 		
		[Column] public float? DT_LCL { get; set; } 	
	}

	[TableName("Line")]
	[PrimaryKey("LineId")]
	[ExplicitColumns]
    public partial class Line : labDB.Record<Line>  
    {		
		[Column] public int LineId { get; set; } 		
		[Column] public int? LineTankId { get; set; } 		
		[Column] public int UnitId { get; set; } 		
		[Column] public int LineNumber { get; set; } 		
		[Column] public int? SystemId { get; set; } 		
		[Column] public int StatusId { get; set; } 		
		[Column] public int ProductCodeId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int PersonId { get; set; } 		
		[Column] public int ConversionId { get; set; } 	
	}

	[TableName("SolutionRecipe")]
	[PrimaryKey("SolutionRecipeId")]
	[ExplicitColumns]
    public partial class SolutionRecipe : labDB.Record<SolutionRecipe>  
    {		
		[Column] public int SolutionRecipeId { get; set; } 		
		[Column] public string SolutionType { get; set; } 	
	}

	[TableName("System")]
	[PrimaryKey("SystemId")]
	[ExplicitColumns]
    public partial class System : labDB.Record<System>  
    {		
		[Column] public int SystemId { get; set; } 		
		[Column] public string Status { get; set; } 		
		[Column("System")] public string _System { get; set; }
	
	}

	[TableName("Extruder")]
	[PrimaryKey("ExtruderId")]
	[ExplicitColumns]
    public partial class Extruder : labDB.Record<Extruder>  
    {		
		[Column] public int ExtruderId { get; set; } 		
		[Column] public int ExtruderType { get; set; } 		
		[Column] public int Nozzle { get; set; } 		
		[Column] public string Color { get; set; } 	
	}

	[TableName("Unit")]
	[PrimaryKey("UnitId")]
	[ExplicitColumns]
    public partial class Unit : labDB.Record<Unit>  
    {		
		[Column] public int UnitId { get; set; } 		
		[Column("Unit")] public string _Unit { get; set; }
	
	}

}



