namespace Words.Web.Core.TagHelpers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    public class EmailTagHelper : TagHelper
    {
        private const string EmailDomain = "contoso.com";
        public string MailTo { get; set; } = null!;

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            string address = $"{MailTo}@{EmailDomain}";
            output.Attributes.SetAttribute("href", $"mailto:{address}");
            output.Content.SetContent(address);
            return Task.CompletedTask;
        }
    }
}
