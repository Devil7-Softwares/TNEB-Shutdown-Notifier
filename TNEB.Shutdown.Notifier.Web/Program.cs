using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.EntityFrameworkCore;
using Quartz;
using TNEB.Shutdown.Notifier.Web.Data;
using TNEB.Shutdown.Notifier.Web.Jobs;
using TNEB.Shutdown.Notifier.Web.Utils;

namespace TNEB.Shutdown.Notifier.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddJsEngineSwitcher(options =>
                options.DefaultEngineName = ChakraCoreJsEngine.EngineName
            )
                .AddChakraCore();

            builder.Services.AddWebOptimizer((pipeline) =>
            {
                pipeline.CompileScssFiles(null, "css/**/*.scss");

                if (builder.Environment.IsProduction())
                {
                    pipeline.MinifyJsFiles("js/**/*.js");
                    pipeline.MinifyCssFiles("css/**/*.css");
                }
            });

            builder.Services.AddQuartz(q =>
            {
                string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    q.UsePersistentStore(s =>
                    {
                        s.UseNewtonsoftJsonSerializer();
                        s.UseProperties = true;
                        s.RetryInterval = TimeSpan.FromSeconds(15);
                        s.UseSqlServer(connectionString);
                    });
                }
            });

            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Configuration.GetValue<bool>("RunMigrations"))
            {
                IServiceScope scope = app.Services.CreateScope();
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseWebOptimizer();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseBasicAuth();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.ScheduleScrapper();

            app.Run();
        }
    }
}
