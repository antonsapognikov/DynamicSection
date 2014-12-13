using System;
using System.IO;
using System.Web;
using San.Web.DynamicSection.Filters;

namespace San.Web.DynamicSection.Modules
{
    public class SectionContextModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        private void OnPreRequestHandlerExecute(object sender, EventArgs args)
        {
            HttpApplication application = (HttpApplication) sender;
            Stream baseFilter = application.Response.Filter;
            application.Response.Filter = new SectionContextFilter(baseFilter);
        }

        public void Dispose()
        {
        }
    }
}