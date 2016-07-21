using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class CasingView : CasingSample
    {
        public string moiststatus { get { return this.SpecClr(m => m.MoistPct); } }
        public string glycstatus { get { return this.SpecClr(m => m.GlyPct); } }

        public List<Run> runs;
        public TagSample layflat;

        public CasingView(int id, int direction) : base(id, direction)
        {
            runs = Run.RunsNow(Stamp, product.ProductCodeId);
            layflat = new TagSample(LineId, "layflat_mm_pv", Stamp.AddHours(-4), Stamp);     // hack to get average over last sample interval  
        }

        public CasingView() { }
    }

    public class Casings
    {
        private static string _samplesByProduct = @"
            select 
                   p.[ProductCodeId]       
                  ,p.[ProductCode]         
                  ,p.[ProductSpec]        
                  ,p.[ReelMoist_Min]           
                  ,p.[ReelMoist_Aim]           
                  ,p.[ReelMoist_Max]           
                  ,p.[Gly_Min]        
                  ,p.[Gly_Aim]                
                  ,p.[Gly_Max]                    
                  ,p.[Oil_Min]                    
                  ,p.[Oil_Aim]                
                  ,p.[Oil_Max]        
                  ,p.[LF_Min]                    
                  ,p.[LF_Aim]                
                  ,p.[LF_Max]         
                  ,p.[LF_UCL]                    
                  ,p.[LF_LCL]         
            from ProductCode p where productcodeid = @0

            SELECT s.[SampleId]
                  ,s.[Scheduled]
                  ,s.[Stamp]
                  ,s.[LineId]
                  ,s.[Note]
                  ,s.[Tech]
                  ,s.[Completed]
                  ,s.[ReelNumber]
                  ,s.[Footage]
                  ,s.[BarCode]
                  ,s.[ParameterId]
                  ,s.[Reading1]
                  ,s.[Reading2]
                  ,s.[Reading3]
                  ,s.[ProcessId]
                  ,s.[SystemId]
                  ,s.[NextScheduled]
                  ,n.[LineNumber]     as Line__LineNumber
                  ,n.[UnitId]         as Line__UnitId
                  ,r.[ReadingId]      as Gly__ReadingId     
                  ,r.[LineId]         as Gly__LineId        
                  ,r.[Stamp]          as Gly__Stamp         
                  ,r.[R1]             as Gly__R1            
                  ,r.[R2]             as Gly__R2                
                  ,r.[R3]             as Gly__R3            
                  ,r.[R4]             as Gly__R4            
                  ,r.[R5]             as Gly__R5            
                  ,r.[Average]        as Gly__Average
                  ,r.[ParameterId]    as Gly__ParameterId
                  ,r.[Operator]       as Gly__Operator    
                  ,r.[EditCount]      as Gly__EditCount
                  ,r.[Scheduled]      as Gly__Scheduled     
                  ,r.[ParameterId]    as Gly__ParameterId
                  ,p.[ProductCodeId]  as _product__ProductCodeId
                  ,p.[ProductCode]    as _product___ProductCode     
                  ,p.[ProductSpec]    as _product__ProductSpec      
                  ,p.[PlastSpec]      as _product__PlastSpec        
                  ,p.[WetLayflat_Aim] as _product__WetLayflat_Aim   
                  ,p.[WetLayflat_Min] as _product__WetLayflat_Min   
                  ,p.[WetLayflat_Max] as _product__WetLayflat_Max   
                  ,p.[Glut_Aim]       as _product__Glut_Aim      
                  ,p.[Glut_Max]       as _product__Glut_Max     
                  ,p.[Glut_Min]       as _product__Glut_Min      
                  ,p.[ReelMoist_Aim]  as _product__ReelMoist_Aim 
                  ,p.[ReelMoist_Min]  as _product__ReelMoist_Min 
                  ,p.[ReelMoist_Max]  as _product__ReelMoist_Max 
                  ,p.[LF_Aim]         as _product__LF_Aim        
                  ,p.[LF_Min]         as _product__LF_Min        
                  ,p.[LF_Max]         as _product__LF_Max        
                  ,p.[LF_LCL]         as _product__LF_LCL        
                  ,p.[LF_UCL]         as _product__LF_UCL        
                  ,p.[OilType]        as _product__OilType       
                  ,p.[Oil_Aim]        as _product__Oil_Aim       
                  ,p.[Oil_Min]        as _product__Oil_Min       
                  ,p.[Oil_Max]        as _product__Oil_Max       
                  ,p.[Gly_Aim]        as _product__Gly_Aim       
                  ,p.[Gly_Min]        as _product__Gly_Min       
                  ,p.[Gly_Max]        as _product__Gly_Max       
              FROM [dbo].[Sample] s
              join ProductCode p on p.productcodeid = s.productcodeid
              left join [Line] n on n.LineId = s.LineId
              left join [dbo].[Reading] r on r.SampleId = s.SampleId
              where s.productcodeid = @0
              order by s.stamp desc
        ";
        public ProductCode product { get; set; }
        public List<CasingView> samples { get; set; }

        public Casings(int productid)
        {
            using (labDB d = new labDB())
            {
                var result = d.FetchMultiple<ProductCode, CasingView>(_samplesByProduct, productid);
                product = result.Item1.SingleOrDefault();
                samples = result.Item2;
            }
        }
    }
}