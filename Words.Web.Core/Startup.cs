namespace Words.Web.Core
{
    using System.Data;
    using System.Text;
    using System.Text.Json;
    using Npgsql;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment WebHostEnvironment {get;set;}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            WordsOptions wordsOptions =
                Configuration
                    .GetSection(WordsOptions.Words)
                    .Get<WordsOptions>();
            services.Configure<WordsOptions>(Configuration.GetSection(WordsOptions.Words));
            services.AddControllersWithViews();
            services.AddScoped<IDbConnection>(sp => {
                NpgsqlConnection connection = new(wordsOptions.ConnectionString);
                connection.Open();
                return connection;
            });
            services.AddSingleton(LoadWordFinders(Path.Combine(WebHostEnvironment.ContentRootPath, "App_Data", "words.json")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(); // ?
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization(); // ?

            app.UseEndpoints(endpoints => // ?
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            AppDomain.CurrentDomain.SetData(
                "DataDirectory",
                Path.Combine(env.ContentRootPath, "App_Data"));
        }

        private static WordFinders LoadWordFinders(string filename)
        {
            string path = Path.Combine("DataDirectory", filename);
            string succinctTreeDataJson = File.ReadAllText(path, Encoding.UTF8);
            Bucket[]? buckets = JsonSerializer.Deserialize<Bucket[]>(succinctTreeDataJson);
            if (buckets is null)
            {
                throw new InvalidOperationException("deserialization failed");
            }

            WordFinders wordFinders = new(buckets);
            return wordFinders;
        }
    }
}
