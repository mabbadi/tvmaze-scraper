using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Nest;
using Hangfire.MySql;
using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using rtl_app.Context;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

        services.Configure<ELKConfiguration>(Configuration.GetSection("ELKConfiguration"));
        services.Configure<ApplicationLogging>(Configuration.GetSection("ApplicationLogging"));
        services.Configure<ApplicationInfo>(Configuration.GetSection("ApplicationInfo"));
        services.Configure<ApiKey>(Configuration.GetSection("ApiKey"));


        services.AddDbContext<RTLConsumerContext>(options =>
        {
            options.UseNpgsql(Configuration.GetConnectionString("ConsumerConnection"));
        });

        services.AddControllers();


        var serviceProvider = services.BuildServiceProvider();
        var ELKConfiguration = serviceProvider.GetService<IOptions<ELKConfiguration>>();
        var ApplicationLogging = serviceProvider.GetService<IOptions<ApplicationLogging>>();
        services.AddElasticsearch(Configuration, ELKConfiguration, ApplicationLogging);

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis"));

        

        services.AddTransient<ITvMazeStorage, ElasticTvMaze>();
        services.AddTransient<IConsumer, Consumer>();

        //configure background timer job
        services.AddHostedService<BackgroundJob>();

        //configure hangfire
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage<MySqlStorage>(new MySqlStorage(Configuration.GetConnectionString("HangfireConnection"), new MySqlStorageOptions())));
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
        });



    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHangfireDashboard("/hangfire", new DashboardOptions { });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


        ScheduleHangfireJobs(env);

    }

    private void ScheduleHangfireJobs(IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            using (var connection = Hangfire.JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

        // RecurringJob.AddOrUpdate<IConsumer>(x =>
        //     x.ProcessDataIncremental(), "*/10 * * * * *");

        RecurringJob.AddOrUpdate<IConsumer>(x =>
            x.LoadUrls(false), "0 0 * * *");
    }
}