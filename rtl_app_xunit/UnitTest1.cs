using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace rtl_app_xunit;
public class ExampleTest
{
    private ITvMazeStorage iTvMazeStorage;

    public ExampleTest()
    {
        iTvMazeStorage = new ElasticTvMaze(null, null, null);
        iTvMazeStorage.TestTvMazeApi(new HttpClient());
    }

    [Fact]
    public async Task TestGetShowAndCast(){
        var res = await iTvMazeStorage.TestGetShowAndCast("1");
        ShowDetails document = JsonConvert.DeserializeObject<ShowDetails>(res);
        Assert.Equal(1, document.id);
        Assert.Equal("Under the Dome", document.name);
    }

/*
    THE TESTST BELOW CAN BE USED TO TEST THE APPLICATION AT RUNTIME WITH FOR EXAMPLE Jenkins OR GitLab CI/CD
    THE FIRST TEST VERIFIES THAT THE APPLICATION DOES RETURN A RESULT WHEN SEARCHING FOR A CERTAIN QUERY, AND PAGINATED
    THE SECOND ONE VERIFIES THAT REQUESTING A WAY TOO BIG PAGE NUMBER THE APPLICATION IS STILL WORKING AND THAT RETURNS AN EMPTY COLLECTION
    THE THIRD TEST VERIFIES THAT SEARCHING FOR NOTHING THE API STILL WORKS AND RETURNS ALL RESULTS, AND PAGINATED
*/

    // [Fact]
    // public async Task TestApiContentFilled()
    // {
    //     // Arrange
    //     var client = new HttpClient();
    //     var url = "http://localhost:4000/TvMaze/search/shows?q=American%20Dad&page=1";

    //     // Act
    //     var response = await client.GetAsync(url);
    //     var content = await response.Content.ReadAsStringAsync();

    //     // Assert
    //     Assert.NotNull(content);
    //     Assert.NotEqual("[]", content);
    // }

    // [Fact]
    // public async Task TestApiEmpty()
    // {
    //     // Arrange
    //     var client = new HttpClient();
    //     var url = "http://localhost:4000/TvMaze/search/shows?q=American%20Dad&page=10000000";

    //     // Act
    //     var response = await client.GetAsync(url);
    //     var content = await response.Content.ReadAsStringAsync();

    //     // Assert
    //     Assert.NotNull(content);
    //     Assert.Equal("[]", content);
    // }

    // [Fact]
    // public async Task TestApiContentFilled2()
    // {
    //     // Arrange
    //     var client = new HttpClient();
    //     var url = "http://localhost:4000/TvMaze/search/shows?q=&page=1";

    //     // Act
    //     var response = await client.GetAsync(url);
    //     var content = await response.Content.ReadAsStringAsync();

    //     // Assert
    //     Assert.NotNull(content);
    //     Assert.NotEqual("[]", content);
    // }
}