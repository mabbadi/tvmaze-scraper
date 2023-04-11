using Microsoft.Extensions.Options;
using Nest;

public record ElasticProduct(IElasticClient elasticClient,
                             IOptions<ApplicationLogging> applicationLogging) : IProductStorage
{

    private void Log(string message, System.ConsoleColor color = ConsoleColor.Yellow)
    {
        if (applicationLogging != null && applicationLogging.Value.logElastic)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{message} - {DateTime.Now}");
            Console.ResetColor(); // reset console color to default
        }
    }
    public async Task AddProduct(Product product)
    {
        // Add product to ELS index
        // Index product dto
        Log("Adding product to elastic.");
        await elasticClient.IndexDocumentAsync(product);
    }

    public async Task<List<Product>> GetAllProducts(string keyword)
    {

        var queries = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();
        if (!String.IsNullOrEmpty(keyword))
        {
            queries.Add(pq => pq
              .Match(m => m
                .Field(f => f.Title)
                  .Query(keyword)));
            queries.Add(pq => pq
              .Match(m => m
                .Field(f => f.Description)
                  .Query(keyword)));
        }

        var result = await elasticClient.SearchAsync<Product>(
                        s => s.Query(q => q
                                .Bool(b => b
                                  .Should(queries))).Size(5000));
        Log($"Returning products from query with keyword {keyword}.");
        return result.Documents.ToList();
    }
}