
// This file was automatically generated by the PetaPoco T4 Template
// Do not make changes directly to this file - edit the template instead
// 
// The following connection settings were used to generate this file
// 
//     Connection String Name: `lab`
//     Provider:               `System.Data.SqlClient`
//     Connection String:      `Data Source=GUYLISTER3546;Initial Catalog=mesdb;Integrated Security=True;Connection Timeout=120`
//     Schema:                 `dbo`
//     Include Views:          `True`

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Test.Models
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
	

	[TableName("Booster")]
	[PrimaryKey("BoosterId")]
	[ExplicitColumns]
    public partial class Booster : labDB.Record<Booster>  
    {		
		[Column] public int BoosterId { get; set; } 		
		[Column] public int SolutionBatchId { get; set; } 		
		[Column] public DateTime? DateTime { get; set; } 	
	}

	[TableName("CasingGroup")]
	[PrimaryKey("CasingGroupId")]
	[ExplicitColumns]
    public partial class CasingGroup : labDB.Record<CasingGroup>  
    {		
		[Column] public int CasingGroupId { get; set; } 		
		[Column] public DateTime DateTime { get; set; } 	
	}

	[TableName("CasingTest")]
	[PrimaryKey("CasingTestId")]
	[ExplicitColumns]
    public partial class CasingTest : labDB.Record<CasingTest>  
    {		
		[Column] public int CasingTestId { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int SystemId { get; set; } 		
		[Column] public int? Reel { get; set; } 		
		[Column] public decimal? Delm { get; set; } 		
		[Column] public decimal? WetWt { get; set; } 		
		[Column] public decimal? DryWt { get; set; } 		
		[Column] public decimal? GlyWetWt { get; set; } 		
		[Column] public decimal? GlyArea { get; set; } 		
		[Column] public decimal? GlySTD { get; set; } 		
		[Column] public decimal? OilArea { get; set; } 		
		[Column] public decimal? Oil { get; set; } 		
		[Column] public DateTime DateTime { get; set; } 		
		[Column] public int? CasingGroupId { get; set; } 		
		[Column] public int? Feet { get; set; } 	
	}

	[TableName("Deck")]
	[PrimaryKey("DeckId")]
	[ExplicitColumns]
    public partial class Deck : labDB.Record<Deck>  
    {		
		[Column] public int DeckId { get; set; } 		
		[Column] public string Name { get; set; } 	
	}

	[TableName("Extruder")]
	[PrimaryKey("ExtruderId", AutoIncrement=false)]
	[ExplicitColumns]
    public partial class Extruder : labDB.Record<Extruder>  
    {		
		[Column] public int ExtruderId { get; set; } 		
		[Column] public int ExtruderType { get; set; } 		
		[Column] public int Nozzle { get; set; } 		
		[Column] public string Color { get; set; } 	
	}

	[TableName("Sample")]
	[PrimaryKey("SampleId")]
	[ExplicitColumns]
    public partial class Sample : labDB.Record<Sample>  
    {		
		[Column] public int SampleId { get; set; } 		
		[Column] public DateTime Scheduled { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int ProductCodeId { get; set; } 		
		[Column] public string Note { get; set; } 		
		[Column] public string Tech { get; set; } 		
		[Column] public DateTime? Completed { get; set; } 		
		[Column] public int ReelNumber { get; set; } 		
		[Column] public int? Footage { get; set; } 		
		[Column] public int? BarCode { get; set; } 		
		[Column] public int? ParameterId { get; set; } 		
		[Column] public int? Reading1 { get; set; } 		
		[Column] public int? Reading2 { get; set; } 		
		[Column] public int? ProcessId { get; set; } 		
		[Column] public int? SystemId { get; set; } 		
		[Column] public DateTime? NextScheduled { get; set; } 		
		[Column] public int? Reading3 { get; set; } 	
	}

	[TableName("Parameter")]
	[PrimaryKey("ParameterId")]
	[ExplicitColumns]
    public partial class Parameter : labDB.Record<Parameter>  
    {		
		[Column] public int ParameterId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public string Scale { get; set; } 		
		[Column] public string Mask { get; set; } 		
		[Column] public string Units { get; set; } 		
		[Column] public string Diary { get; set; } 		
		[Column] public int Count { get; set; } 		
		[Column] public string Icon { get; set; } 		
		[Column] public bool ReadNow { get; set; } 		
		[Column] public string Cells { get; set; } 	
	}

	[TableName("OilSTD")]
	[PrimaryKey("OilSTDId")]
	[ExplicitColumns]
    public partial class OilSTD : labDB.Record<OilSTD>  
    {		
		[Column] public int OilSTDId { get; set; } 		
		[Column] public DateTime? DateTime { get; set; } 		
		[Column] public int? Concentration { get; set; } 		
		[Column] public double Area { get; set; } 		
		[Column] public int? CasingGroupId { get; set; } 	
	}

	[TableName("Conversion")]
	[PrimaryKey("ConversionId", AutoIncrement=false)]
	[ExplicitColumns]
    public partial class Conversion : labDB.Record<Conversion>  
    {		
		[Column] public int ConversionId { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int ProductCodeId { get; set; } 		
		[Column] public int SystemId { get; set; } 		
		[Column] public int ExtruderId { get; set; } 		
		[Column] public DateTime Scheduled { get; set; } 		
		[Column] public DateTime Started { get; set; } 		
		[Column] public DateTime Completed { get; set; } 		
		[Column] public int FinishFootage { get; set; } 		
		[Column] public bool Exempt { get; set; } 		
		[Column] public string ExemptCode { get; set; } 		
		[Column] public string Note { get; set; } 	
	}

	[TableName("ProductCodeTx")]
	[PrimaryKey("ProductCodeTxId", AutoIncrement=false)]
	[ExplicitColumns]
    public partial class ProductCodeTx : labDB.Record<ProductCodeTx>  
    {		
		[Column] public int ProductCodeTxId { get; set; } 		
		[Column] public int ProductCodeId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public string UserId { get; set; } 		
		[Column] public string Delta { get; set; } 	
	}

	[TableName("Reading")]
	[PrimaryKey("ReadingId")]
	[ExplicitColumns]
    public partial class Reading : labDB.Record<Reading>  
    {		
		[Column] public int ReadingId { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int? R1 { get; set; } 		
		[Column] public int? R2 { get; set; } 		
		[Column] public int? R3 { get; set; } 		
		[Column] public int? R4 { get; set; } 		
		[Column] public int? R5 { get; set; } 		
		[Column] public int? Average { get; set; } 		
		[Column] public int ParameterId { get; set; } 		
		[Column] public string Operator { get; set; } 		
		[Column] public int EditCount { get; set; } 		
		[Column] public DateTime Scheduled { get; set; } 		
		[Column] public int SampleId { get; set; } 	
	}

	[TableName("RecipeReading")]
	[PrimaryKey("RecipeReadingId")]
	[ExplicitColumns]
    public partial class RecipeReading : labDB.Record<RecipeReading>  
    {		
		[Column] public int RecipeReadingId { get; set; } 		
		[Column] public int SolutionRecipeId { get; set; } 		
		[Column] public int ReadingId { get; set; } 		
		[Column] public string LineColor { get; set; } 		
		[Column] public double? Low { get; set; } 		
		[Column] public double? High { get; set; } 	
	}

	[TableName("Series")]
	[PrimaryKey("SeriesId")]
	[ExplicitColumns]
    public partial class Series : labDB.Record<Series>  
    {		
		[Column] public int SeriesId { get; set; } 		
		[Column] public string Title { get; set; } 		
		[Column] public string Field { get; set; } 		
		[Column] public string YLabel { get; set; } 		
		[Column] public string Legend { get; set; } 		
		[Column] public int? Height { get; set; } 		
		[Column] public string ForeGround { get; set; } 	
	}

	[TableName("Slide")]
	[PrimaryKey("SlideId")]
	[ExplicitColumns]
    public partial class Slide : labDB.Record<Slide>  
    {		
		[Column] public int SlideId { get; set; } 		
		[Column] public int DeckId { get; set; } 		
		[Column] public string Name { get; set; } 		
		[Column] public string FileNameSuffix { get; set; } 		
		[Column] public string FileFormat { get; set; } 	
	}

	[TableName("SlideSeries")]
	[PrimaryKey("SlideSeriesId")]
	[ExplicitColumns]
    public partial class SlideSeries : labDB.Record<SlideSeries>  
    {		
		[Column] public int SlideSeriesId { get; set; } 		
		[Column] public int SlideId { get; set; } 		
		[Column] public int SeriesId { get; set; } 	
	}

	[TableName("SolutionBatch")]
	[PrimaryKey("SolutionBatchId")]
	[ExplicitColumns]
    public partial class SolutionBatch : labDB.Record<SolutionBatch>  
    {		
		[Column] public int SolutionBatchId { get; set; } 		
		[Column] public DateTime DateTime { get; set; } 		
		[Column] public int SolutionRecipeId { get; set; } 		
		[Column] public int OperatorId { get; set; } 		
		[Column] public string CoA { get; set; } 		
		[Column] public int SystemId { get; set; } 		
		[Column] public DateTime? Completed { get; set; } 	
	}

	[TableName("SolutionRecipe")]
	[PrimaryKey("SolutionRecipeId")]
	[ExplicitColumns]
    public partial class SolutionRecipe : labDB.Record<SolutionRecipe>  
    {		
		[Column] public int SolutionRecipeId { get; set; } 		
		[Column] public string SolutionType { get; set; } 	
	}

	[TableName("ReadingField")]
	[PrimaryKey("ReadingFieldId")]
	[ExplicitColumns]
    public partial class ReadingField : labDB.Record<ReadingField>  
    {		
		[Column] public int ReadingFieldId { get; set; } 		
		[Column] public string FieldName { get; set; } 		
		[Column] public string Title { get; set; } 		
		[Column] public string LineColor { get; set; } 		
		[Column] public int? Axis { get; set; } 		
		[Column] public double? Low { get; set; } 		
		[Column] public double? High { get; set; } 	
	}

	[TableName("SolutionTest")]
	[PrimaryKey("SolutionTestId")]
	[ExplicitColumns]
    public partial class SolutionTest : labDB.Record<SolutionTest>  
    {		
		[Column] public int SolutionTestId { get; set; } 		
		[Column] public int SolutionBatchId { get; set; } 		
		[Column] public DateTime DateTime { get; set; } 		
		[Column] public decimal? SolutionRecipeId { get; set; } 		
		[Column] public decimal? CMC { get; set; } 		
		[Column] public decimal? DensitySetPoint { get; set; } 		
		[Column] public decimal? ConsoleDensity { get; set; } 		
		[Column] public decimal? pHSetPoint { get; set; } 		
		[Column] public decimal? Viscoscity { get; set; } 		
		[Column] public decimal? Temperature { get; set; } 		
		[Column] public decimal? TitrationMLs { get; set; } 		
		[Column("NaOCl Pump Set")] public decimal? NaOCl_Pump_Set { get; set; }
		
		[Column("NaOCl Flow")] public int? NaOCl_Flow { get; set; }
		
		[Column] public decimal? MeasuredDensity { get; set; } 		
		[Column] public decimal? ConsolepH { get; set; } 		
		[Column] public decimal? MeasuredpH { get; set; } 		
		[Column] public decimal? Conductivity { get; set; } 		
		[Column("Acid Pump Output")] public decimal? Acid_Pump_Output { get; set; }
		
		[Column("Booster Pump Output")] public int? Booster_Pump_Output { get; set; }
		
		[Column] public decimal? Glycerin { get; set; } 		
		[Column] public decimal? Hypochlorite { get; set; } 		
		[Column] public decimal? CasingGlycerin { get; set; } 		
		[Column] public decimal? Feed { get; set; } 		
		[Column] public int? Steam { get; set; } 	
	}

	[TableName("ReadingTag")]
	[PrimaryKey("ReadingTagId")]
	[ExplicitColumns]
    public partial class ReadingTag : labDB.Record<ReadingTag>  
    {		
		[Column] public int ReadingTagId { get; set; } 		
		[Column] public int ReadingFieldId { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int TagId { get; set; } 	
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
	}

	[TableName("Unit")]
	[PrimaryKey("UnitId")]
	[ExplicitColumns]
    public partial class Unit : labDB.Record<Unit>  
    {		
		[Column] public int UnitId { get; set; } 		
		[Column("Unit")] public string _Unit { get; set; }
	
	}

	[TableName("Status")]
	[PrimaryKey("StatusId")]
	[ExplicitColumns]
    public partial class Status : labDB.Record<Status>  
    {		
		[Column] public int StatusId { get; set; } 		
		[Column] public string Code { get; set; } 		
		[Column] public string Description { get; set; } 		
		[Column] public string Icon { get; set; } 		
		[Column] public string Color { get; set; } 	
	}

	[TableName("LineHistory")]
	[PrimaryKey("LineHistoryId")]
	[ExplicitColumns]
    public partial class LineHistory : labDB.Record<LineHistory>  
    {		
		[Column] public int LineHistoryId { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int? LineTankId { get; set; } 		
		[Column] public int UnitId { get; set; } 		
		[Column] public int LineNumber { get; set; } 		
		[Column] public int? SystemId { get; set; } 		
		[Column] public int StatusId { get; set; } 		
		[Column] public int ProductCodeId { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int UserId { get; set; } 		
		[Column] public string Comment { get; set; } 		
		[Column] public DateTime Taken { get; set; } 	
	}

	[TableName("LineOperation")]
	[ExplicitColumns]
    public partial class LineOperation : labDB.Record<LineOperation>  
    {		
		[Column] public decimal INDAY { get; set; } 		
		[Column] public string INUNIT { get; set; } 		
		[Column] public decimal INLINE { get; set; } 		
		[Column] public decimal INSHFT { get; set; } 		
		[Column] public string STCODE { get; set; } 		
		[Column] public decimal INTIME { get; set; } 		
		[Column] public string RSCODE { get; set; } 		
		[Column] public string INPRD { get; set; } 		
		[Column] public DateTime? stamp { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public int? ProductCodeId { get; set; } 	
	}

	[TableName("LineStatus")]
	[ExplicitColumns]
    public partial class LineStatus : labDB.Record<LineStatus>  
    {		
		[Column] public string INUNT { get; set; } 		
		[Column] public string INLIN { get; set; } 		
		[Column] public decimal INDAY { get; set; } 		
		[Column] public string INPRD { get; set; } 		
		[Column] public string CARTN { get; set; } 		
		[Column] public string INSID { get; set; } 		
		[Column] public decimal INLSQ { get; set; } 		
		[Column] public string INLST { get; set; } 		
		[Column] public string INREL { get; set; } 		
		[Column] public decimal INBSP { get; set; } 		
		[Column] public decimal INSAM { get; set; } 		
		[Column] public int LineId { get; set; } 		
		[Column] public string Status { get; set; } 		
		[Column] public string Reason { get; set; } 		
		[Column] public DateTime Stamp { get; set; } 		
		[Column] public int? ProductCodeId { get; set; } 	
	}

	[TableName("ProductCode")]
	[PrimaryKey("ProductCodeId")]
	[ExplicitColumns]
    public partial class ProductCode : labDB.Record<ProductCode>  
    {		
		[Column] public int ProductCodeId { get; set; } 		
		[Column("ProductCode")] public string _ProductCode { get; set; }
		
		[Column] public string ProductSpec { get; set; } 		
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

}



