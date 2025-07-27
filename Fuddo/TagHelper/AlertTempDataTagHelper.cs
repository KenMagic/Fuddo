using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Fuddo.TagHelpers
{
    [HtmlTargetElement("tempdata-alert")]
    public class AlertTempDataTagHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var tempData = ViewContext.TempData;
            var content = "";

            if (tempData["Success"] != null)
            {
                content += $@"
<div class='alert alert-success alert-dismissible fade show' role='alert'>
    <i class='bi bi-check-circle-fill me-2'></i> {tempData["Success"]}
    <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
</div>";
            }

            if (tempData["Error"] != null)
            {
                content += $@"
<div class='alert alert-danger alert-dismissible fade show' role='alert'>
    <i class='bi bi-exclamation-triangle-fill me-2'></i> {tempData["Error"]}
    <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
</div>";
            }

            output.TagName = null; // Không render thẻ <tempdata-alert>
            output.Content.SetHtmlContent(content);
        }
    }
}
