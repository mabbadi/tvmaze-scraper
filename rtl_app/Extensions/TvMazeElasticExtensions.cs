using System.Text;
using Nest;

public static class TvMazeElasticExtensions
{
    
    public static void AddDefaultMappings(ConnectionSettings settings, string indexName)
    {
        settings.DefaultMappingFor<ShowDetails>(m => m.IndexName($"{indexName}-show-details"));
        settings.DefaultMappingFor<Person>(m => m.IndexName($"{indexName}-person-details"));
    }

    public static void DeleteIndex(IElasticClient client, string indexName)
    {
        var res = client.Indices.Delete($"{indexName}-show-details");
        Console.WriteLine($"{indexName}-show-details index deletion: {res.DebugInformation}");

        var res1 = client.Indices.Delete($"{indexName}-person-details");
        Console.WriteLine($"{indexName}-person-details index deletion: {res1.DebugInformation}");
    }

    public static void CreateIndex(IElasticClient client, string indexName)
    {
        if (!client.Indices.Exists($"{indexName}-show-details").Exists)
        {
            var createIndexResponse = client.Indices.Create($"{indexName}-show-details",
                index => index.Map<ShowDetails>(x => x.AutoMap())
            );
            Console.WriteLine($"{indexName}-show-details create: {createIndexResponse.DebugInformation}");
        }
        var mappingRes = client.Indices.PutMapping<ShowDetails>(s => s.AutoMap());
        Console.WriteLine($"{indexName}-show-details put mapping: {mappingRes.DebugInformation}");
        

        if (!client.Indices.Exists($"{indexName}-person-details").Exists)
        {
            var createIndexResponse = client.Indices.Create($"{indexName}-person-details",
                index => index.Map<Person>(x => x.AutoMap())
            );
            Console.WriteLine($"{indexName}-person-details create: {createIndexResponse.DebugInformation}");
        }
        var mappingRes1 = client.Indices.PutMapping<Person>(s => s.AutoMap());
        Console.WriteLine($"{indexName}-person-details put mapping: {mappingRes1.DebugInformation}");
    }

}
