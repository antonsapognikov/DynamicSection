using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace San.Web.DynamicSection.Context
{
    public class SectionContextResolver
    {
        public const string StylesheetFileTemplate = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
        public const string StylesheetBlockTemplate = "<style>{0}</style>";
        public const string ScriptFileTemplate = "<script type=\"text/javascript\" src=\"{0}\"></script>";
        public const string ScriptBlockTemplate = "<script type=\"text/javascript\">{0}</script>";

        private static readonly Func<string[], IHtmlString> StylesheetFileResolver = paths =>
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
            StringBuilder builder = new StringBuilder(paths.Length);
            foreach (string path in paths)
            {
                builder.AppendLine(string.Format(StylesheetFileTemplate, UrlHelper.GenerateContentUrl(path, context)));
            }
            return new HtmlString(builder.ToString());
        };

        private static readonly Func<string[], IHtmlString> ScriptFileResolver = paths =>
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
            StringBuilder builder = new StringBuilder(paths.Length);
            foreach (string path in paths)
            {
                builder.AppendLine(string.Format(ScriptFileTemplate, UrlHelper.GenerateContentUrl(path, context)));
            }
            return new HtmlString(builder.ToString());
        };

        private static readonly Func<string[], IHtmlString> BlockResolver = paths =>
        {
            StringBuilder builder = new StringBuilder(paths.Length);
            foreach (string path in paths)
            {
                builder.AppendLine(path);
            }
            return new HtmlString(builder.ToString());
        };

        private static readonly SectionContextResolver Resolver = new SectionContextResolver();

        public static SectionContextResolver Instance
        {
            get { return Resolver; }
        }

        private readonly Dictionary<SectionContextItemType, Func<string[], IHtmlString>> resolvers =
            new Dictionary<SectionContextItemType, Func<string[], IHtmlString>>();

        private SectionContextResolver()
        {
            resolvers.Add(SectionContextItemType.StylesheetFile, StylesheetFileResolver);
            resolvers.Add(SectionContextItemType.ScriptFile, ScriptFileResolver);
            resolvers.Add(SectionContextItemType.StylesheetBlock, BlockResolver);
            resolvers.Add(SectionContextItemType.ScriptBlock, BlockResolver);
        }

        public void SetResolver(SectionContextItemType type, Func<string[], IHtmlString> resolver)
        {
            if (resolvers.ContainsKey(type))
            {
                resolvers[type] = resolver;
            }
            else
            {
                resolvers.Add(type, resolver);
            }
        }

        public IHtmlString Resolve(SectionContextItemType type, string[] path)
        {
            return resolvers[type](path);
        }
    }
}