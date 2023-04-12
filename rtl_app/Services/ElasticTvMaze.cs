using System.Globalization;
using Hangfire;
using Nest;
using Newtonsoft.Json;
using rtl_app.Context;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Extensions.Options;
using System.Reflection;

public record ElasticTvMaze(IElasticClient elasticClient,
                            RTLConsumerContext context,
                            ILogger<ElasticTvMaze> logger,
                            IOptions<ApplicationLogging> applicationLogging) : ITvMazeStorage
{

    private void Log(string message, System.ConsoleColor color = ConsoleColor.Yellow)
    {
        logger.Log(message, () => applicationLogging != null && applicationLogging.Value.logElastic, color);

    }

    public async Task LoadData(List<ShowDetails> documents)
    {
        if (documents.Count() == 0)
        {
            return;
        }

        if (documents.Count() == 1)
        {
            await elasticClient.IndexAsync<ShowDetails>(documents.First(), null);
            Log($"Document with id {documents.First().id} loaded");
            return;
        }

        ProcessBulk<ShowDetails>(documents);
        ProcessBulk<Person>(documents.SelectMany(x => x._embedded.cast).Select(c => c.person).ToList());

    }


    private void ProcessBulk<T>(List<T> documents) where T : class
    {

        // Split the list into batches of 10
        var batches = documents.Select((doc, index) => new { doc, index })
                               .GroupBy(x => x.index / 10)
                               .Select(g => g.Select(x => x.doc));

        Log($"Start batch indexing");
        // Loop through each batch and create the corresponding BulkRequest
        foreach (var batch in batches)
        {
            var bulkRequest = new BulkRequest()
            {
                Operations = new List<IBulkOperation>()
            };

            foreach (var document in batch)
            {
                bulkRequest.Operations.Add(new BulkIndexOperation<T>(document));
            }

            var response = elasticClient.Bulk(bulkRequest);

            // Check the response for errors
            if (response.Errors)
            {
                var unprocessedEpisodes = new List<T>();
                foreach (var action in response.Items)
                {

                    if (action.Error != null)
                    {
                        var failedDocument = batch.ElementAt(Int32.Parse(action.Index));
                        unprocessedEpisodes.Add(failedDocument);
                        Log($"Bulk insert error: {action.Status} {action.Result}", ConsoleColor.Red);

                    }
                }

                //we serialize the documents/episodes that failed to enter the index, so later we can retry to upload them again

                context.UnprocessedEpisodesDetails.AddRange(
                  unprocessedEpisodes.Select(e =>
                  {
                      var id = "";
                      Type type = typeof(T);
                      FieldInfo field = type.GetField("id");
                      if (field != null)
                      {
                          // The type T has a field named "id"
                          // You can access it using:
                          id = $"{field.GetValue(e)}";
                      }
                      return new UnprocessedEpisodeDetails()
                      {
                          Id = id,
                          RawData = JsonConvert.SerializeObject(e)
                      };
                  })
                );
                context.SaveChanges();
            }
        }
        Log("End batch indexing");
    }

    public async Task LoadMockData()
    {
        string workingDirectory = Environment.CurrentDirectory;
        var sampleJSONPath = System.IO.Path.Join(workingDirectory, "sampleJSON.json");
        var sampleJSON = File.ReadAllText(sampleJSONPath);
        if (!String.IsNullOrEmpty(sampleJSON))
        {
            Log("Loading mock documents from file");
            ShowDetails document = JsonConvert.DeserializeObject<ShowDetails>(sampleJSON);
            await LoadData(new List<ShowDetails>(new ShowDetails[] { document }));
        }
    }
    public async Task<ShowDetails> SearchSingle(string query, int fuzziness)
    {
        var response = await elasticClient.SearchAsync<ShowDetails>(s => s
            .Sort(sort => sort.Descending("_score")) // Sort by score in descending order
            .Query(q => q
              .MultiMatch(m => m
                .FuzzyTranspositions()
                .Fuzziness(fuzziness == 1 ? Fuzziness.Auto : Fuzziness.EditDistance(fuzziness))
                .Lenient()
                .Fields(fs =>
                  fs.Field(f => f.summary))
                .Query(query)
              )
            )
            .Size(0) // Set the number of hits to 0, so we only get the aggregation results
              .Aggregations(a => a
                  .TopHits("top_hits", th => th
                      .Size(1) // Set the number of top hits to 1
                      .Sort(sort => sort.Descending("_score")) // Sort by score in descending order
                  )
              )
            );
        var topHit = response.Aggregations.TopHits("top_hits").Documents<ShowDetails>().FirstOrDefault();
        return topHit!;

    }

    public async Task<List<ShowDetails>> Search(string query, int fuzziness, int pageNumber = 1, int pageSize = 10)
    {
        var response = await elasticClient.SearchAsync<ShowDetails>(s => s
            .From((pageNumber - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
              .MultiMatch(m => m
                .FuzzyTranspositions()
                .Fuzziness(fuzziness == 1 ? Fuzziness.Auto : Fuzziness.EditDistance(fuzziness))
                .Lenient()
                .Fields(fs => fs.Field(f => f.summary))
                .Query(query)
              )));
        return response.AllResultingSources()
            // .Select(p =>
            // {
            //     p._embedded.cast = p._embedded.cast.OrderByDescending(c => c.person.birthday).ToList();
            //     return p;
            // })
            .ToList();
    }

    public async Task<List<(int, string)>> GetAllShowsUrl()
    {
        List<string> showsId = await GetShowsId();
        var result = new List<(int, string)>();


        foreach (var id in showsId)
        {
            // var url = $"https://api.tvmaze.com/schedule?country=US&date=2014-12-01"; //example
            var url = $"https://api.tvmaze.com/shows/{id}?embed[]=cast";
            result.Add((Int32.Parse(id), url));
        }

        return result;
    }

    async Task<List<string>> GetShowsId()
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                var response = await httpClient.GetAsync("http://api.tvmaze.com/updates/shows");
                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    return new List<string>();
                }
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                Dictionary<string, int> dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(responseBody);
                return new List<string>(dict.Keys);

            }
            catch (HttpRequestException ex)
            {
                Log("Getting shows ids failed. " + ex.Message, ConsoleColor.Red);
                return new List<string>();

            }
        }
    }

    public async Task<List<string>> FindMissingUrls(List<(int, string)> ids1)
    {
        var res = await elasticClient.SearchAsync<ShowDetails>(s => s.Query(q => q.Bool(b => b.Must(mn => mn.Ids(ids => ids.Values(ids1.Select(u => u.Item1.ToString())))))));
        var idsMatching = res.Hits.Select(h => h.Source.id).ToDictionary(x => x);
        return ids1.Select(id => !idsMatching.ContainsKey(id.Item1) ? id.Item2 : null).Where(i => i != null).ToList()!;
    }

    private HttpClient testClient { get; set; }
    public void TestTvMazeApi(HttpClient client)
    {
        testClient = client;
    }

    public async Task<string> TestGetShowAndCast(string id)
    {
        var response = await testClient.GetAsync($"https://api.tvmaze.com/shows/{id}??embed[]=cast");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}