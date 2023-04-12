using System.ComponentModel.DataAnnotations;


public class ConsumerQueueItem
{
    
    public int Id { get; set; }
    public string UrlToProcess { get; set; }
}

public class UnprocessedEpisodeDetails
{
    [Key]
    public string Id { get; set; }
    public string RawData { get; set; }
}