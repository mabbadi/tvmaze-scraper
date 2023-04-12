using Microsoft.AspNetCore;
using Microsoft.Extensions.CommandLineUtils;
using Nest;
using rtl_app.Context;
using rtl_app;
using Microsoft.EntityFrameworkCore;

namespace rtl
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = CreateWebHostBuilder(args).Build();
            var app = new CommandLineApplication();
            app.HelpOption("-? | -h | --help");

            app.Command("es-update", (command) =>
                  {
                      command.Description = "Create ElasticSearch indices";
                      command.HelpOption("-? | -h | --help");

                      command.OnExecute(() =>
                      {
                          var services = server.Services.CreateScope().ServiceProvider;
                          var elasticClient = services.GetService<IElasticClient>();
                          var configuration = services.GetService<IConfiguration>();
                          var defaultIndex = configuration["ELKConfiguration:index"];
                          ElasticSearchExtensions.CreateIndex(elasticClient, defaultIndex);
                          return 0;
                      });
                  });

            app.Command("es-drop", (command) =>
            {
                command.Description = "Delete ElasticSearch indices";
                command.HelpOption("-? | -h | --help");

                command.OnExecute(() =>
                {
                    var services = server.Services.CreateScope().ServiceProvider;
                    var elasticClient = services.GetService<IElasticClient>();
                    var configuration = services.GetService<IConfiguration>();
                    var defaultIndex = configuration["ELKConfiguration:index"];
                    ElasticSearchExtensions.DeleteIndex(elasticClient, defaultIndex);
                    return 0;
                });
            });

            app.Command("db-migrate", (command) =>
            {
                command.Description = "Migrate the database";
                command.HelpOption("-? | -h | --help");

                command.OnExecute(() =>
          {
              using (var scope = server.Services.CreateScope())
              {
                  var services = scope.ServiceProvider;
                  var context = services.GetRequiredService<RTLConsumerContext>();
                  context.Database.Migrate();
                  Console.WriteLine("Database migrated");
                  return 0;
              }
          });
            });

            app.Command("db-drop", (command) =>
            {
                command.Description = "Migrate the database";
                command.HelpOption("-? | -h | --help");

                command.OnExecute(() =>
          {
              using (var scope = server.Services.CreateScope())
              {
                  var services = scope.ServiceProvider;
                  var context = services.GetRequiredService<RTLConsumerContext>();
                  context.Database.EnsureDeleted();
                  Console.WriteLine("Database dropped");
                  return 0;
              }
          });
            });            



            app.OnExecute(() =>
            {
                server.Run();
                return 0;
            });

            app.Execute(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                    config.SetBasePath(ctx.HostingEnvironment.ContentRootPath)
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddCommandLine(args)
                      .AddEnvironmentVariables()
                    )
                .UseStartup<Startup>()
                .UseUrls("http://*:5000");
        }
    }
}
