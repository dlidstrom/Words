using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Words.Web
{
    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.AddPerIndividualFile<StylesheetBundle>("Content/css");
            bundles.AddPerIndividualFile<ScriptBundle>("Content/js");
        }
    }
}