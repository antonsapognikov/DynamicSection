using System;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using San.Web.DynamicSection.Context;

namespace San.Web.DynamicSection.Extensions
{
    public static class SectionContextHelper
    {
        private const string CurrentContext = "74a3cedc-ee14-4f2d-89fc-d78ee939e549";

        public static SectionContext BeginSection(this HtmlHelper htmlHelper, string sectionName, int order = 0)
        {
            HttpContextBase context = htmlHelper.ViewContext.HttpContext;
            SectionContext section = new SectionContext(context, htmlHelper.ViewContext.Writer, sectionName, order);
            context.Items[CurrentContext] = section;
            return section;
        }

        public static void AddStylesheetFile(this HtmlHelper htmlHelper, string stylesheetFile, bool renderOnAjax = true)
        {
            SectionContext section = htmlHelper.GetSection();
            section.AddStylesheetFile(stylesheetFile, renderOnAjax);
        }

        public static void AddStylesheetBlock(this HtmlHelper htmlHelper, Func<dynamic, HelperResult> stylesheetTemplate, bool renderOnAjax = true)
        {
            SectionContext section = htmlHelper.GetSection();
            section.AddStylesheetBlock(stylesheetTemplate, renderOnAjax);
        }

        public static void AddStylesheetBlock(this HtmlHelper htmlHelper, string block, bool renderOnAjax = true)
        {
            SectionContext section = htmlHelper.GetSection();
            section.AddStylesheetBlock(block, renderOnAjax);
        }

        public static void IgnoreStylesheetFile(this HtmlHelper htmlHelper, string stylesheetFile)
        {
            SectionContext section = htmlHelper.GetSection();
            section.IgnoreScriptFile(stylesheetFile);
        }

        public static void AddScriptFile(this HtmlHelper htmlHelper, string scriptFile, bool renderOnAjax = true)
        {
            SectionContext section = htmlHelper.GetSection();
            section.AddScriptFile(scriptFile, renderOnAjax);
        }

        public static void AddScriptBlock(this HtmlHelper htmlHelper, Func<dynamic, HelperResult> scriptTemplate, bool renderOnAjax = true)
        {
            SectionContext section = htmlHelper.GetSection();
            section.AddScriptBlock(scriptTemplate, renderOnAjax);
        }

        public static void AddScriptBlock(this HtmlHelper htmlHelper, string block, bool renderOnAjax = true)
        {
            SectionContext section = htmlHelper.GetSection();
            section.AddScriptBlock(block, renderOnAjax);
        }

        public static void IgnoreScriptFile(this HtmlHelper htmlHelper, string scriptFile)
        {
            SectionContext section = htmlHelper.GetSection();
            section.IgnoreScriptFile(scriptFile);
        }

        public static void EndSection(this HtmlHelper htmlHelper)
        {
            SectionContext section = htmlHelper.GetSection();
            section.Dispose();
            htmlHelper.ViewContext.HttpContext.Items.Remove(CurrentContext);
        }

        public static IHtmlString RenderSection(this HtmlHelper htmlHelper, string sectionName)
        {
            return SectionContext.Render(sectionName);
        }

        private static SectionContext GetSection(this HtmlHelper htmlHelper)
        {
            HttpContextBase context = htmlHelper.ViewContext.HttpContext;
            SectionContext section = context.Items[CurrentContext] as SectionContext;
            if (section == null)
            {
                throw new InvalidOperationException("No SectionContext in HttpContext.Items. Call Html.BeginSection() to create a SectionContext.");
            }
            return section;
        }
    }
}