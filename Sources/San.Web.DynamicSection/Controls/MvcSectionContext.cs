using System.Web;
using System.Web.Mvc;

namespace San.Web.DynamicSection.Controls
{
    public class MvcSectionContext
    {
        public const string TagName = "sectioncontext";

        public string Id { get; private set; }

        public MvcSectionContext(string id)
        {
            Id = id;
        }

        public IHtmlString ToHtmlString()
        {
            TagBuilder builder = new TagBuilder(TagName);
            builder.Attributes["id"] = Id;
            return new HtmlString(builder.ToString(TagRenderMode.SelfClosing));
        }
    }
}