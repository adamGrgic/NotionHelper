
namespace NotionHelper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Register HttpClient
            builder.Services.AddHttpClient();

            builder.Services.AddControllers();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.MapControllers();
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
