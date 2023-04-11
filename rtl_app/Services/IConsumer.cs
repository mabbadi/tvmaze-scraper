public interface IConsumer {
    Task LoadUrls(bool forceLoad = false);
    Task ProcessDataIncremental();
}
