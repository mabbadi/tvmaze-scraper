using System.Security.Cryptography.X509Certificates;
using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;

public static class ElasticSearchExtensions
{
    public static void AddElasticsearch(
        this IServiceCollection services, IConfiguration configuration, IOptions<ELKConfiguration> ELKConfiguration, IOptions<ApplicationLogging> applicationLogging)
    {

        if (ELKConfiguration == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Cannot connect to elastic. Configuration missing.");
            Console.ResetColor(); // reset console color to default
            return;
        }
        var url = ELKConfiguration.Value.url;
        var defaultIndex = ELKConfiguration.Value.index;

        var settings = new ConnectionSettings(new Uri(url))//.BasicAuthentication(userName, pass)
            .PrettyJson()
            //.EnableApiVersioningHeader()
            .DefaultFieldNameInferrer(p => p);

        AddDefaultMappings(settings, defaultIndex);
        
        if (applicationLogging != null && applicationLogging.Value.logElasticFull)
        {
            settings.EnableDebugLogging();
        }


        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);

        CreateIndex(client, defaultIndex);
        Console.WriteLine("AddElasticsearch index done");
    }

    public static void AddDefaultMappings(ConnectionSettings settings, string indexName)
    {
        Console.WriteLine("Running AddDefaultMappings");
        TvMazeElasticExtensions.AddDefaultMappings(settings, indexName);
    }

    public static void DeleteIndex(IElasticClient client, string indexName)
    {
        Console.WriteLine("Running DeleteIndex");
        TvMazeElasticExtensions.DeleteIndex(client, indexName);
    }

    public static void CreateIndex(IElasticClient client, string indexName)
    {
        Console.WriteLine("Running CreateIndex");
        TvMazeElasticExtensions.CreateIndex(client, indexName);
    }

    public static IEnumerable<T> AllResultingSources<T>(this ISearchResponse<T> response) where T : class => response.Hits.Select(h => h.Source);

}

public static class ConnectionSettingsExtensionMethods
{
    public static ConnectionSettings EnableDebugLogging(this ConnectionSettings connectionSettings)
    {
        return connectionSettings
          .DisableDirectStreaming()
          .OnRequestCompleted(apiCallDetails =>
          {
              if (apiCallDetails.RequestBodyInBytes != null)
              {
                  System.Console.WriteLine(
                  $"{apiCallDetails.HttpMethod} {apiCallDetails.Uri} " +
                  $"{Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes)}"
                );
              }
              else
              {
                  System.Console.WriteLine($"{apiCallDetails.HttpMethod} {apiCallDetails.Uri}");
              }

              if (apiCallDetails.ResponseBodyInBytes != null)
              {
                  System.Console.WriteLine(
                  $"Status: {apiCallDetails.HttpStatusCode}" +
                  $"{Encoding.UTF8.GetString(apiCallDetails.ResponseBodyInBytes)}"
                );
              }
              else
              {
                  System.Console.WriteLine($"Status: {apiCallDetails.HttpStatusCode}");
              }
          })
          ;
    }
}