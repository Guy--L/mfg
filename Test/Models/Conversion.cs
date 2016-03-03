using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using NPoco;
using Omu.ValueInjecter;
using System.Diagnostics;

namespace Test.Models
{
    public enum ConversionStates
    {
        Scheduled,          // 0 1 2
        Started,            // 0 2 3
        Completed,          // 3
        Ignored             // 3
    };

    public partial class Conversion
    {
        private static string _all = @"
            select c.conversionid, c.solutionrecipeid, c.linetxid, c.extruderid, c.finishfootage, c.note, c.endstateid, c.conversionstate,
                n.lineid, n.personid, n.stamp, n.comment, n.linetankid, n.unitid, n.linenumber, n.systemid, n.statusid, n.productcodeid
            from conversion c
            {0} join linetxn n on c.linetxid = n.linetxid
            ";

        public int SystemId { get; set; }
        public int StatusId { get; set; }
        public int ProductCodeId { get; set; }
        public int LineId { get; set; }
        public int PersonId { get; set; }
        public string Comment { get; set; }

        public string System { get { return Models.System.Systems[SystemId]; } }
        public string SolutionType { get { return SolutionRecipe.Solutions[SolutionRecipeId]; } }
        public Status Status { get { return Status.state[StatusId]; } }
        public Line line { get; set; }
        public ProductCode product { get; set; }

        private List<string> buttons = new List<string>()
        {
            "<button class='btn btn-xs btnconv btn-danger btnignore IgnoreConversion' title='Does not affect line setting' data-id='{0}'><i class='fa fa-ban'></i> Ignore</button> ",
            "<button class='btn btn-xs btnconv btn-info btnstart StartConversion' title='Mark line as being in conversion' data-id='{0}'><i class='fa fa-random'></i> Start</button> ",
            "<button class='btn btn-xs btnconv btn-success btncomplete CompleteConversion' title='Apply conversion settings to line' data-id='{0}'><i class='fa fa-arrow-up'></i> Complete</button> ",
            "<button class='btn btn-xs btnconv btn-primary btnundo UndoConversion' id='undoconversion'><i class='fa fa-undo' data-id='{0}'></i> Undo Recent Conversion</button> "
        };

        private string[] transit = new string[] { "012", "023", "3", "3" };

        public string Action(int ConversionId)
        {
            var edges = transit[(int)ConversionState];
            var action = "";
            foreach (char e in edges)
                action += buttons[e - '0'];

            return string.Format(action, ConversionId);
        }

        public string Color {
            get {
                var colors = Extruder.Colors[ExtruderId].Split(' ');
                var tint = colors.Select(c => "<span style='color: " + c + (c=="White"?"; background-color: black;": ";")+"'>" + c + "</span>");
                return string.Join(" ", tint);
            }
        }

        public string Icon { get { return StatusId==0?"":Status.state[StatusId].Iconic(); } }

        public Conversion() { }

        public Conversion(LineTx ln)
        {
            this.InjectFrom(ln);
        }

        public Conversion(int id)
        {
            if (id != 0)
            {
                var c = new Conversion();
                using (labDB d = new labDB())
                {
                    c = d.Fetch<Conversion>(string.Format(_all, "") + " where c.conversionid = @0 order by stamp desc", id).SingleOrDefault();
                }
                this.InjectFrom(c);
                return;
            }
            Comment = "";
            ProductCodeId = 0;
        }

        public static List<Conversion>Conversions()
        {
            using (labDB d = new labDB())
            {
                return d.Fetch<Conversion>(string.Format(_all, "left"));
            }
        }

        //public void SyncState()
        //{
        //    var now = DateTime.Now;
        //    state = ConversionState.Scheduled;
        //    if (Completed < now)
        //        state = Started > now.AddYears(200) ? ConversionState.Ignored : ConversionState.Completed;
        //    else if (Started < now)
        //        state = ConversionState.Started;
        //}

        //public string StateLabel
        //{
        //    get {  return state.ToString().ToLower(); }
        //}

        /// <summary>
        /// T-SQL maxdate and DateTime.MaxValue are ever so slightly different
        /// </summary>
        //public void Future()
        //{
        //    var diff = DateTime.MaxValue - Completed;
        //    Completed = (diff.TotalMinutes < 1) ? DateTime.MaxValue : Completed;

        //    if (!Started.HasValue) Started = DateTime.MaxValue;
        //    else
        //    {
        //        diff = DateTime.MaxValue - Started.Value;
        //        Started = (diff.TotalMinutes < 1) ? DateTime.MaxValue : Started;
        //    }
        //}

        public string Complete(int person)
        {
            try {
                var now = DateTime.Now;
                var line = new Line(this);
                line.StatusId = StatusId;
                line.Stamp = now;
                line.PersonId = person;
                line.Update();
                LineTxId = LineTx.LatestTx(LineId);
                ConversionState = ConversionStates.Completed;
                Update();
            }
            catch (Exception e)
            { 
                return "Error: " + e.Message;
            }
            return "Line conversion completed on "+ Unit.id2line[LineId];
        }

        public string Start(int person)
        {
            try
            {
                var now = DateTime.Now;
                var line = new Line(this);
                line.StatusId = Status.statuses["Conversion"];
                line.Stamp = now;
                line.PersonId = person;
                line.Update();
                LineTxId = LineTx.LatestTx(LineId);
                ConversionState = ConversionStates.Started;
                Update();
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
            return "Line conversion started on "+Unit.id2line[LineId];
        }

        public string Ignore(int person)
        {
            ConversionState = ConversionStates.Ignored;          
            Update();
            return "Conversion ignored for "+Unit.id2line[LineId];
        }
    }

    public class Conversions
    {
        public List<Conversion> conversions { get; set; }

        public Conversions()
        {
            conversions = Conversion.Conversions();
        }

        public Conversions(bool recent)
        {

        }
    }

    public class UndoConversionView
    {
        public Conversion prior { get; set; }
        public Conversion current { get; set; }

        public UndoConversionView() { }

        public UndoConversionView(int id)
        {
            //var c = new Conversion(id);
            //var query = string.Format(Conversions._undoconversion, c.LineId, c.ConversionId);
            //var u = new Conversions(query);

            //current = u.conversions.First();
            //current.ProductCodeId = current.product.ProductCodeId;
            //prior = null;
            //if (u.conversions.Count() > 1)
            //{
            //    prior = u.conversions.Last();
            //    prior.ProductCodeId = prior.product.ProductCodeId;
            //    prior.LineId = prior.line.LineId;
            //}
        }

        public string Commit(int personid)
        {
            try {
                current.ModifiedColumns = new Dictionary<string, bool>();
                current.Update();

                var line = new Line(prior);
                line.PersonId = personid;
                line.Stamp = DateTime.Now;
                line.Update();
            }
            catch(Exception e)
            {
                var msg = "Error: " + e.Message;
#if DEBUG
                msg += "\n" + e.StackTrace;
#endif
                return msg;
            }
            return "Conversion reverted";
        }
    }

    public class ConversionView
    {
        public Conversion c { get; set; }
        public List<System> systems { get; set; }
        public List<Status> statuses { get; set; }
        public SelectList products { get; set; }
        public SelectList recipes { get; set; }
        public SelectList extruders { get; set; }
        public List<Line> lines { get; set; }

        public ConversionView() { }

        public ConversionView(int id)
        {
            var lns = new Lines();
            lines = lns.lines;

            using (var db = new labDB())
            {
                c = id > 0 ? db.SingleOrDefaultById<Conversion>(id) : new Conversion() {
                    SolutionRecipeId = 0
//                    ,state = Conversion.ConversionState.Scheduled
                };
                systems = db.Fetch<System>(System._active);
                statuses = db.Fetch<Status>();
                products = new SelectList(db.Fetch<ProductCode>(" order by productcode, productspec"), "ProductCodeId", "CodeSpec", c.ProductCodeId);
                recipes = new SelectList(db.Fetch<SolutionRecipe>(), "SolutionRecipeId", "SolutionType", c.SolutionRecipeId);
                extruders = new SelectList(db.Fetch<Extruder>(), "ExtruderId", "Color", c.ExtruderId);
            }
        }
    }
}