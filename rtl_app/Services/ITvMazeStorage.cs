public interface ITvMazeStorage {
    public Task LoadMockData();
    public Task LoadData(List<ShowDetails> episodes); // method returns not processed episodes
    public Task<List<(int, string)>> GetAllShowsUrl();
    public Task<List<ShowDetails>> Search(string query, int fuzziness, int pageNumber = 1, int pageSize = 10);
    public Task<List<string>> FindMissingUrls(List<(int, string)> ids);
    public void TestTvMazeApi(HttpClient client);
    public Task<string> TestGetShowAndCast(string id);
}