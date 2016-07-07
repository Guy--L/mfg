using System.Web.Optimization;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Test.App_Start.BootstrapBundleConfig), "RegisterBundles")]

namespace Test.App_Start
{
    public class BootstrapBundleConfig
    {
        public static void RegisterBundles()
        {
            // Add @Styles.Render("~/Content/bootstrap/base") in the <head/> of your _Layout.cshtml view
            // For Bootstrap theme add @Styles.Render("~/Content/bootstrap/theme") in the <head/> of your _Layout.cshtml view
            // Add @Scripts.Render("~/bundles/bootstrap") after jQuery in your _Layout.cshtml view
            // When <compilation debug="true" />, MVC4 will render the full readable version. When set to <compilation debug="false" />, the minified version will be rendered automatically
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery-datetimepicker.js",
                "~/Scripts/jquery.inputmask/inputmask.js",
                "~/Scripts/jquery.inputmask/inputmask.extensions.js",
                "~/Scripts/jquery.inputmask/inputmask.date.extensions.js",
                "~/Scripts/jquery.inputmask/inputmask.numeric.extensions.js",
                "~/Scripts/jquery.inputmask/jquery.inputmask.js",
                "~/Scripts/jquery-barcode.js",
                "~/Scripts/jquery.fittext.js",
                "~/Scripts/respond.js",
                "~/Scripts/TwitterBootstrapMvcJs.js"
                ));

            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap/base").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css",
                "~/Content/font-awesome-animation.css"
                ));
            //BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap/theme").Include(
            //    "~/Content/bootstrap/bootstrap-theme.css"));
        }
    }
}
