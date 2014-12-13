using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using San.Web.DynamicSection.Context;

namespace San.Web.DynamicSection.Test
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SectionContextResolver.Instance.SetResolver(SectionContextItemType.StylesheetFile, Styles.Render);
            SectionContextResolver.Instance.SetResolver(SectionContextItemType.ScriptFile, Scripts.Render);
        }
    }
}
