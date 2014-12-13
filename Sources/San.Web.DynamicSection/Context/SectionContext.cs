using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using San.Web.DynamicSection.Controls;

namespace San.Web.DynamicSection.Context
{
    public class SectionContext : IDisposable
    {      
        private const string ContextIdsKey = "69079a82-e876-4f54-a83d-da061a21b52b";
        private const string ContextRenderedIdsKey = "3b4006a1-108c-4aab-962a-bba2bb495aa9";
        private const string ContextItemsKey = "3939e3b0-82f3-4183-82c4-946977f5b349";
        private const string ContextIgnoredItemsKey = "ce2ea12e-6264-4e56-a78b-b713565e4072";

        private readonly HttpContextBase context;
        private readonly TextWriter writer;

        private readonly Dictionary<SectionContextItemType, Stack<string>> items = new Dictionary<SectionContextItemType, Stack<string>>();
        private readonly Dictionary<SectionContextItemType, Stack<string>> ignoredItems = new Dictionary<SectionContextItemType, Stack<string>>();

        private bool IsAjaxRequest
        {
            get { return context.Request.IsAjaxRequest(); }
        }

        public string Name { get; private set; }

        public Guid Id { get; private set; }

        public int Order { get; private set; }

        public SectionContext(HttpContextBase httpContext, TextWriter textWriter, string sectionName, int order)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            context = httpContext;
            writer = textWriter;
            Name = sectionName;
            Id = RegisterSection(Name);
            Order = order;
        }

        public static IHtmlString Render(string sectionName)
        {
            Guid sectionId = RegisterSection(sectionName);
            AddRenderedIdToContext(sectionId);
            return new MvcSectionContext(sectionId.ToString()).ToHtmlString();
        }

        public static List<Guid> GetRendered()
        {
            return new List<Guid>(GetRenderedIdsFromContext());
        }

        public static IHtmlString GetHtml(string sectionName)
        {
            Dictionary<string, Guid> ids = GetIdsFromContext();
            return ids.ContainsKey(sectionName) ? GetHtml(ids[sectionName]) : null;
        }

        public static IHtmlString GetHtml(Guid sectionId)
        {
            Dictionary<Guid, Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>> items = GetItemsFromContext();
            if (!items.ContainsKey(sectionId))
                return new HtmlString(string.Empty);

            Dictionary<Guid, Dictionary<SectionContextItemType, List<string>>> ignoredItems = GetIgnoredItemsFromContext();

            StringBuilder builder = new StringBuilder();
            foreach (SectionContextItemType type in items[sectionId].Keys)
            {
                IEnumerable<string> ignoredPath = ignoredItems.ContainsKey(sectionId) && ignoredItems[sectionId].ContainsKey(type)
                    ? ignoredItems[sectionId][type]
                    : Enumerable.Empty<string>();
                IEnumerable<string> path = items[sectionId][type]
                    .OrderBy(item => item.Item2).Select(item => item.Item1).Except(ignoredPath).Distinct();
                builder.Append(SectionContextResolver.Instance.Resolve(type, path.ToArray()));
            }
            return new HtmlString(builder.ToString());
        }

        public static void Remove(string sectionName)
        {
            Dictionary<string, Guid> ids = GetIdsFromContext();
            if (ids.ContainsKey(sectionName))
            {
                Remove(ids[sectionName]);
            }
        }

        public static void Remove(Guid sectionId)
        {
            Dictionary<string, Guid> ids = GetIdsFromContext();
            if (ids.Any(pair => pair.Value == sectionId))
            {
                ids.Remove(ids.First(pair => pair.Value == sectionId).Key);
            }
            
            HashSet<Guid> renderedIds = GetRenderedIdsFromContext();
            renderedIds.Remove(sectionId);

            Dictionary<Guid, Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>> items = GetItemsFromContext();
            items.Remove(sectionId);

            Dictionary<Guid, Dictionary<SectionContextItemType, List<string>>> ignoredItems = GetIgnoredItemsFromContext();
            ignoredItems.Remove(sectionId);
        }

        public void Add(SectionContextItemType type, string file, bool renderOnAjax = true)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (!IsAjaxRequest || renderOnAjax)
            {
                AddItem(type, file);
            }
        }

        public void AddStylesheetFile(string stylesheet, bool renderOnAjax = true)
        {
            Add(SectionContextItemType.StylesheetFile, stylesheet, renderOnAjax);
        }

        public void AddStylesheetBlock(Func<dynamic, HelperResult> stylesheetTemplate, bool renderOnAjax = true)
        {
            Add(SectionContextItemType.StylesheetBlock, stylesheetTemplate(null).ToString(), renderOnAjax);
        }

        public void AddStylesheetBlock(string block, bool renderOnAjax = true)
        {
            Add(SectionContextItemType.StylesheetBlock, string.Format(SectionContextResolver.StylesheetBlockTemplate, block), renderOnAjax);
        }

        public void AddScriptFile(string script, bool renderOnAjax = true)
        {
            Add(SectionContextItemType.ScriptFile, script, renderOnAjax);
        }

        public void AddScriptBlock(Func<dynamic, HelperResult> scriptTemplate, bool renderOnAjax = true)
        {
            Add(SectionContextItemType.ScriptBlock, scriptTemplate(null).ToString(), renderOnAjax);
        }

        public void AddScriptBlock(string block, bool renderOnAjax = true)
        {
            Add(SectionContextItemType.ScriptBlock, string.Format(SectionContextResolver.ScriptBlockTemplate, block), renderOnAjax);
        }

        public void Ignore(SectionContextItemType type, string file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            AddIgnoredItem(type, file);
        }

        public void IgnoreStylesheetFile(string stylesheet)
        {
            Ignore(SectionContextItemType.StylesheetFile, stylesheet);
        }

        public void IgnoreScriptFile(string script)
        {
            Ignore(SectionContextItemType.ScriptFile, script);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<SectionContextItemType, Stack<string>> pair in items)
            {
                foreach (string item in pair.Value)
                {
                    AddItemToContext(Id, pair.Key, item, Order);
                }
            }

            foreach (KeyValuePair<SectionContextItemType, Stack<string>> pair in ignoredItems)
            {
                foreach (string item in pair.Value)
                {
                    AddIgnoredItemToContext(Id, pair.Key, item);
                }
            }

            if (IsAjaxRequest)
            {
                writer.Write(GetHtml(Id));
            }
        }

        private static Guid RegisterSection(string sectionName)
        {
            Dictionary<string, Guid> ids = GetIdsFromContext();
            if (ids.ContainsKey(sectionName))
            {
                return ids[sectionName];
            }
            Guid sectionId = Guid.NewGuid();
            ids.Add(sectionName, sectionId);
            return sectionId;
        }

        private static Dictionary<string, Guid> GetIdsFromContext()
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
            if (context.Items.Contains(ContextIdsKey))
            {
                return context.Items[ContextIdsKey] as Dictionary<string, Guid>;
            }
            Dictionary<string, Guid> ids = new Dictionary<string, Guid>();
            context.Items[ContextIdsKey] = ids;
            return ids;
        }

        private static HashSet<Guid> GetRenderedIdsFromContext()
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
            if (context.Items.Contains(ContextRenderedIdsKey))
            {
                return context.Items[ContextRenderedIdsKey] as HashSet<Guid>;
            }
            HashSet<Guid> ids = new HashSet<Guid>();
            context.Items[ContextRenderedIdsKey] = ids;
            return ids;
        }

        private static void AddRenderedIdToContext(Guid sectionId)
        {
            HashSet<Guid> ids = GetRenderedIdsFromContext();
            ids.Add(sectionId);
        }

        private static Dictionary<Guid, Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>> GetItemsFromContext()
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
            if (context.Items.Contains(ContextItemsKey))
            {
                return context.Items[ContextItemsKey]
                    as Dictionary<Guid, Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>>;
            }
            var items = new Dictionary<Guid, Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>>();
            context.Items[ContextItemsKey] = items;
            return items;
        }

        private static void AddItemToContext(Guid sectionId, SectionContextItemType type, string item, int order)
        {
            Dictionary<Guid, Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>> items = GetItemsFromContext();
            if (!items.ContainsKey(sectionId))
            {
                items.Add(sectionId, new Dictionary<SectionContextItemType, Stack<Tuple<string, int>>>());
            }
            if (!items[sectionId].ContainsKey(type))
            {
                items[sectionId].Add(type, new Stack<Tuple<string, int>>());
            }
            items[sectionId][type].Push(new Tuple<string, int>(item, order));
        }

        private static Dictionary<Guid, Dictionary<SectionContextItemType, List<string>>> GetIgnoredItemsFromContext()
        {
            HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
            if (context.Items.Contains(ContextIgnoredItemsKey))
            {
                return context.Items[ContextIgnoredItemsKey]
                    as Dictionary<Guid, Dictionary<SectionContextItemType, List<string>>>;
            }
            var items = new Dictionary<Guid, Dictionary<SectionContextItemType, List<string>>>();
            context.Items[ContextIgnoredItemsKey] = items;
            return items;
        }

        private static void AddIgnoredItemToContext(Guid sectionId, SectionContextItemType type, string item)
        {
            Dictionary<Guid, Dictionary<SectionContextItemType, List<string>>> items = GetIgnoredItemsFromContext();
            if (!items.ContainsKey(sectionId))
            {
                items.Add(sectionId, new Dictionary<SectionContextItemType, List<string>>());
            }
            if (!items[sectionId].ContainsKey(type))
            {
                items[sectionId].Add(type, new List<string>());
            }
            items[sectionId][type].Add(item);
        }

        private void AddItem(SectionContextItemType type, string item)
        {
            if (!items.ContainsKey(type))
            {
                items.Add(type, new Stack<string>());
            }
            items[type].Push(item);
        }

        private void AddIgnoredItem(SectionContextItemType type, string item)
        {
            if (!ignoredItems.ContainsKey(type))
            {
                ignoredItems.Add(type, new Stack<string>());
            }
            items[type].Push(item);
        }
    }
}