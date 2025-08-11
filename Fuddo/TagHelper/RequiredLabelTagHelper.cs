using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace Fuddo.TagHelpers
{
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class RequiredLabelTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var metadata = For.Metadata;
            bool isRequired = metadata.ValidatorMetadata
                .Any(v => v is RequiredAttribute);

            if (isRequired)
            {
                // Thêm dấu * đỏ vào cuối label
                var content = output.Content.GetContent();
                output.Content.SetHtmlContent($"{content} <span class=\"text-danger\">*</span>");
            }
        }
    }
}
