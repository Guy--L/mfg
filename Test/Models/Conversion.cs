using System.Collections.Generic;

namespace Test.Models
{
    public partial class Conversion
    {
        public string System { get; set; }
        public string Color { get; set; }
        public Line line { get; set; }
        public ProductCode product { get; set; }
    }

    public class Conversions
    {
        public List<Conversion> conversions { get; set; }

        public static string _all = @"
          SELECT c.[ConversionId]
                  ,c.[Scheduled]
                  ,c.[Started]
                  ,c.[Completed]
                  ,c.[FinishFootage]
                  ,c.[Exempt]
                  ,c.[ExemptCode]
                  ,c.[Note]
                  ,e.[Color] 
                  ,l.[Stamp]
                  ,l.[LineId]
                  ,l.[UnitId]
                  ,l.[LineNumber]
                  ,s.[SystemId]
                  ,s.[System]
                  ,p.[ProductCodeId]
                  ,p.[ProductCode]
                  ,p.[ProductSpec]
                  ,p.[PlastSpec]
                  ,p.[WetLayflat_Aim]
                  ,p.[WetLayflat_Min]
                  ,p.[WetLayflat_Max]
                  ,p.[Glut_Aim]
                  ,p.[Glut_Max]
                  ,p.[Glut_Min]
                  ,p.[ReelMoist_Aim]
                  ,p.[ReelMoist_Min]
                  ,p.[ReelMoist_Max]
                  ,p.[LF_Aim]
                  ,p.[LF_Min]
                  ,p.[LF_Max]
                  ,p.[LF_LCL]
                  ,p.[LF_UCL]
                  ,p.[OilType]
                  ,p.[Oil_Aim]
                  ,p.[Oil_Min]
                  ,p.[Oil_Max]
                  ,p.[Gly_Aim]
                  ,p.[Gly_Min]
                  ,p.[Gly_Max]
              from [dbo].[Conversion] c
              join [dbo].[Line] l on c.LineId = l.LineId
              left join [dbo].[Extruder] e on e.ExtruderId = c.ExtruderId
              left join [dbo].[Status] i on i.StatusId = l.StatusId
              left join [dbo].[ProductCode] p on p.ProductCodeId = l.ProductCodeId
              left join [dbo].[System] s on s.SystemId = l.SystemId
        ";

        public static string _pending = _all + @"
              where c.LineId = @0 and c.Started is null
              order by c.Scheduled
        ";

        public Conversions()
        {
            using (var labdb = new labDB())
            {
                conversions = labdb.Fetch<Conversion, Line, ProductCode, Conversion>(
                    (c, l, p) =>
                    {
                        c.product = p ?? new ProductCode() { _ProductCode = "00?00", ProductCodeId = 0 };
                        c.line = l;
                        return c;
                    },
                    _all);
            }
        }
    }
}