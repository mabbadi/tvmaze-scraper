using System.Text;
using Nest;

public static class ProductsElasticExtensions
{
    
    public static void AddDefaultMappings(ConnectionSettings settings, string indexName)
    {
        settings.DefaultMappingFor<Product>(m => m.IndexName($"{indexName}-product"));
    }

    public static void DeleteIndex(IElasticClient client, string indexName)
    {
        var res = client.Indices.Delete($"{indexName}-product");
        Console.WriteLine($"{indexName}-product index deletion: {res.DebugInformation}");
    }

    public static void CreateIndex(IElasticClient client, string indexName)
    {
        if (!client.Indices.Exists($"{indexName}-product").Exists)
        {
            var createIndexResponse = client.Indices.Create($"{indexName}-product",
                index => index.Map<Product>(x => x.AutoMap())
            );
            Console.WriteLine($"{indexName}-product create: {createIndexResponse.DebugInformation}");
        }
        var mappingRes = client.Indices.PutMapping<Product>(s => s.AutoMap());
        Console.WriteLine($"{indexName}-product put mapping: {mappingRes.DebugInformation}");
    }

}
