using System.ComponentModel.DataAnnotations;

public class Consumer
{
    
    public int Id { get; set; }
    public bool Busy { get; set; }
    public DateTime BusySince { get; set; }
}

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