using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Fuddo.TagHelpers
{
    [HtmlTargetElement("pagination")]
    public class PaginationTagHelper : TagHelper
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string BaseUrl { get; set; } = "";
        public string QueryString { get; set; } = ""; 

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "nav";
            output.TagMode = TagMode.StartTagAndEndTag;

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination justify-content-center custom-pagination");

            void AddPageItem(int page, string text, bool disabled = false, bool active = false)
            {
                var li = new TagBuilder("li");
                li.AddCssClass("page-item");
                if (disabled) li.AddCssClass("disabled");
                if (active) li.AddCssClass("active");

                var a = new TagBuilder("a");
                a.AddCssClass("page-link");
                a.Attributes["href"] = $"{BaseUrl}?{QueryString}&page={page}";
                a.InnerHtml.Append(text);

                li.InnerHtml.AppendHtml(a);
                ul.InnerHtml.AppendHtml(li);
            }

            AddPageItem(CurrentPage - 1, "Trước", CurrentPage == 1);

            for (int i = 1; i <= TotalPages; i++)
            {
                AddPageItem(i, i.ToString(), false, i == CurrentPage);
            }

            AddPageItem(CurrentPage + 1, "Tiếp", CurrentPage == TotalPages);

            output.Content.SetHtmlContent(ul);
        }
    }
}
