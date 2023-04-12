using System.Net;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using rtl_app.Context;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Diagnostics;
using StackExchange.Redis;

public record Consumer(RTLConsumerContext context,
                          ITvMazeStorage iTvMazeStorage,
                          IConnectionMultiplexer _redis,
                          ILogger<Consumer> logger,
                          IOptions<ApplicationInfo> applicationInfo,
                          IOptions<ApplicationLogging> applicationLogging) : IConsumer
//assumption is that this consumer processes TVMazeEpisodes
{
    private void Log(string message, System.ConsoleColor color = ConsoleColor.Yellow)
    {
        logger.Log(message, () => applicationLogging != null && applicationLogging.Value.logConsumer, color);
    }
    public async Task LoadUrls(bool forceLoad = false)
    {
        var urlsAll = await iTvMazeStorage.GetAllShowsUrl();
        var urlsMissing = await iTvMazeStorage.FindMissingUrls(urlsAll);
        if (forceLoad == false && context.ConsumerQueue.Count() > 0)
        {
            //background process still not done processing urls/shows
            return;
        }
        var consumerUrls = urlsMissing.Select(u => new ConsumerQueueItem() { UrlToProcess = u });
        context.ConsumerQueue.AddRange(consumerUrls);
        context.SaveChanges();
    }
    public async Task<string> GetContainerNameAsync(string containerId)
    {
        var client = new DockerClientConfiguration().CreateClient();

        var container = await client.Containers.InspectContainerAsync(containerId);

        return container.Name;
    }
    public async Task ProcessDataIncremental()
    {

        if (applicationInfo != null && applicationInfo.Value.IsMainApplication == false)
        {
            //coming from test
            return;
        }

        var db = _redis.GetDatabase();
        var acquiredLock = db.StringSet("my-lock-key", "locked", TimeSpan.FromSeconds(20), When.NotExists);
        if (acquiredLock)
        {
            try
            {
                await ConsumeLogic();
            }
            finally
            {
                // Release the lock
                Log("Release the lock");
                db.KeyDelete("my-lock-key");
            }
        }
        else
        {
            // Another container has the lock
            Log("Unable to acquire lock");
        }
    }

    private async Task ConsumeLogic()
    {
        Log($"About to start ProcessData");
        try
        {

            var topTen = context.ConsumerQueue.Take(20).ToList();
            var processedUrls = new List<ConsumerQueueItem>();
            var shows = new List<ShowDetails>();

            await Task.WhenAll(topTen.Select(doer =>
                    Task.Run(async () =>
                    {
                        await Task.Delay(new Random().Next(100, 1000)); //wait a small amount of time between 1/10 second and 1 second. might not need this line, but better not send 20 calls in parallel
                        await Do(doer.UrlToProcess,
                            (res) =>
                            {
                                //if ok deserialize result
                                if (applicationLogging != null && applicationLogging.Value.logBackgroundJobServiceUrls)
                                {
                                    Log($"Sucessfully processed {doer.UrlToProcess}");
                                }

                                ShowDetails document = JsonConvert.DeserializeObject<ShowDetails>(res);
                                processedUrls.Add(doer); //we remove the url from the urls to process
                                shows.Add(document); //we pass the parsed documents to iTvMazeStorage for further processing
                            },
                            (error) =>
                            {
                                //if error show error. Also we do not add the document 
                                Log($"Error while processing URL {doer.UrlToProcess}.\nError is: {error}\nSkipping it and processing it next time.", ConsoleColor.Red);
                            });
                    })));
            //remove ok urls
            context.ConsumerQueue.RemoveRange(processedUrls);
            context.SaveChanges();
            //load documents associated to ok urls in iTvMazeStorage -> load into elastic for example
            await iTvMazeStorage.LoadData(shows);
            Log($"ProcessData done.");

        }
        catch (Exception e)
        {
            Log($"Consumer error. " + e.Message, ConsoleColor.Red);

        }
    }

    async Task Do(string url, Action<string> onDone, Action<string> onError)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    return;
                }
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                onDone(responseBody);
            }
            catch (HttpRequestException ex)
            {
                onError(ex.Message);
            }
        }
    }
}